using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.EntityFrameworkCore;
using Sikkhaloy.LocalData.Entities;
using Sikkhaloy.Shared.Auth;
using Sikkhaloy.Shared.Classes;
using Sikkhaloy.Shared.Students;
using Sikkhaloy.Shared.Sync;

namespace Sikkhaloy.LocalData.Sync;

public sealed partial class SyncEngine
{
    public async Task<ClassStructureDto> GetClassStructureAsync(int schoolId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var classes = await db.Classes.AsNoTracking()
            .Where(x => x.SchoolID == schoolId)
            .OrderBy(x => x.SortOrder)
            .ThenBy(x => x.Name)
            .Select(x => new SchoolClassDto
            {
                LocalId = x.LocalId,
                ClassID = x.ClassID,
                Name = x.Name,
                SortOrder = x.SortOrder,
                SyncStatus = x.SyncStatus
            })
            .ToListAsync(cancellationToken);

        return new ClassStructureDto
        {
            Classes = classes,
            Groups = await MapPartsAsync(db.ClassGroups.AsNoTracking().Where(x => x.SchoolID == schoolId), cancellationToken),
            Sections = await MapPartsAsync(db.ClassSections.AsNoTracking().Where(x => x.SchoolID == schoolId), cancellationToken),
            Shifts = await MapPartsAsync(db.ClassShifts.AsNoTracking().Where(x => x.SchoolID == schoolId), cancellationToken),
            Joins = await db.ClassJoins.AsNoTracking()
                .Where(x => x.SchoolID == schoolId)
                .OrderBy(x => x.JoinID)
                .Select(x => new ClassJoinDto
                {
                    LocalId = x.LocalId,
                    JoinID = x.JoinID,
                    ClassID = x.ClassID,
                    SubjectGroupID = x.SubjectGroupID,
                    SectionID = x.SectionID,
                    ShiftID = x.ShiftID,
                    GroupName = x.GroupName,
                    SectionName = x.SectionName,
                    ShiftName = x.ShiftName,
                    SyncStatus = x.SyncStatus
                })
                .ToListAsync(cancellationToken)
        };
    }

    public async Task FlushClassStructureAsync(SessionSnapshot session, string accessToken, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(accessToken) || !await _api.PingAsync(cancellationToken))
            return;

        await PushAsync(session, accessToken, cancellationToken);
        await PullClassStructureAsync(session, accessToken, cancellationToken);
        await RefreshPendingAsync(cancellationToken);
        StateChanged?.Invoke();
    }

    public async Task SaveClassAsync(SchoolClassDto dto, SessionSnapshot session, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        if (dto.LocalId == Guid.Empty)
            dto.LocalId = Guid.NewGuid();

        var existing = await db.Classes.FirstOrDefaultAsync(x => x.LocalId == dto.LocalId, cancellationToken);
        var operation = existing is null ? SyncOperation.Create : SyncOperation.Update;
        dto.SyncStatus = existing is null ? SyncStatus.PendingCreate : SyncStatus.PendingUpdate;
        dto.Name = dto.Name.Trim();

        if (existing is null)
        {
            if (dto.ClassID == 0)
                dto.ClassID = NextTempId(await db.Classes.Select(x => x.ClassID).ToListAsync(cancellationToken));
            db.Classes.Add(new LocalSchoolClass
            {
                LocalId = dto.LocalId,
                ClassID = dto.ClassID,
                SchoolID = session.SchoolID,
                Name = dto.Name,
                SortOrder = dto.SortOrder,
                SyncStatus = dto.SyncStatus
            });
        }
        else
        {
            existing.Name = dto.Name;
            existing.SortOrder = dto.SortOrder;
            existing.SyncStatus = dto.SyncStatus;
            dto.ClassID = existing.ClassID;
        }

        Queue(db, dto.LocalId, EntityTypes.Class, operation, ToMutation(dto));
        await db.SaveChangesAsync(cancellationToken);
        await RefreshPendingAsync(cancellationToken);
        StateChanged?.Invoke();
    }

    public async Task DeleteClassAsync(Guid localId, SessionSnapshot session, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var row = await db.Classes.FirstOrDefaultAsync(x => x.LocalId == localId, cancellationToken);
        if (row is null)
            return;

        var payload = ToMutation(new SchoolClassDto
        {
            LocalId = row.LocalId,
            ClassID = row.ClassID,
            Name = row.Name,
            SortOrder = row.SortOrder
        });

        if (row.SyncStatus == SyncStatus.PendingCreate || row.ClassID <= 0)
        {
            DropOutbox(db, row.LocalId);
            db.ClassGroups.RemoveRange(db.ClassGroups.Where(x => x.ClassID == row.ClassID));
            db.ClassSections.RemoveRange(db.ClassSections.Where(x => x.ClassID == row.ClassID));
            db.ClassShifts.RemoveRange(db.ClassShifts.Where(x => x.ClassID == row.ClassID));
            db.ClassJoins.RemoveRange(db.ClassJoins.Where(x => x.ClassID == row.ClassID));
            db.Classes.Remove(row);
        }
        else
        {
            db.Classes.Remove(row);
            Queue(db, row.LocalId, EntityTypes.Class, SyncOperation.Delete, payload);
        }

        await db.SaveChangesAsync(cancellationToken);
        await RefreshPendingAsync(cancellationToken);
        StateChanged?.Invoke();
    }

    public async Task SaveClassPartAsync(string entityType, ClassPartDto dto, SessionSnapshot session, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        if (dto.LocalId == Guid.Empty)
            dto.LocalId = Guid.NewGuid();
        dto.Name = dto.Name.Trim();
        dto.SyncStatus = SyncStatus.PendingUpdate;

        if (entityType == EntityTypes.ClassGroup)
        {
            var row = await db.ClassGroups.FirstOrDefaultAsync(x => x.LocalId == dto.LocalId, cancellationToken);
            var op = row is null ? SyncOperation.Create : SyncOperation.Update;
            dto.SyncStatus = row is null ? SyncStatus.PendingCreate : SyncStatus.PendingUpdate;
            if (row is null)
            {
                if (dto.ServerId == 0)
                    dto.ServerId = NextTempId(await db.ClassGroups.Select(x => x.SubjectGroupID).ToListAsync(cancellationToken));
                db.ClassGroups.Add(new LocalClassGroup
                {
                    LocalId = dto.LocalId,
                    SubjectGroupID = dto.ServerId,
                    SchoolID = session.SchoolID,
                    ClassID = dto.ClassID,
                    Name = dto.Name,
                    SyncStatus = dto.SyncStatus
                });
            }
            else
            {
                row.Name = dto.Name;
                row.SyncStatus = dto.SyncStatus;
                dto.ServerId = row.SubjectGroupID;
                dto.ClassID = row.ClassID;
            }

            Queue(db, dto.LocalId, EntityTypes.ClassGroup, op, ToMutation(dto));
        }
        else if (entityType == EntityTypes.ClassSection)
        {
            var row = await db.ClassSections.FirstOrDefaultAsync(x => x.LocalId == dto.LocalId, cancellationToken);
            var op = row is null ? SyncOperation.Create : SyncOperation.Update;
            dto.SyncStatus = row is null ? SyncStatus.PendingCreate : SyncStatus.PendingUpdate;
            if (row is null)
            {
                if (dto.ServerId == 0)
                    dto.ServerId = NextTempId(await db.ClassSections.Select(x => x.SectionID).ToListAsync(cancellationToken));
                db.ClassSections.Add(new LocalClassSection
                {
                    LocalId = dto.LocalId,
                    SectionID = dto.ServerId,
                    SchoolID = session.SchoolID,
                    ClassID = dto.ClassID,
                    Name = dto.Name,
                    SyncStatus = dto.SyncStatus
                });
            }
            else
            {
                row.Name = dto.Name;
                row.SyncStatus = dto.SyncStatus;
                dto.ServerId = row.SectionID;
                dto.ClassID = row.ClassID;
            }

            Queue(db, dto.LocalId, EntityTypes.ClassSection, op, ToMutation(dto));
        }
        else
        {
            var row = await db.ClassShifts.FirstOrDefaultAsync(x => x.LocalId == dto.LocalId, cancellationToken);
            var op = row is null ? SyncOperation.Create : SyncOperation.Update;
            dto.SyncStatus = row is null ? SyncStatus.PendingCreate : SyncStatus.PendingUpdate;
            if (row is null)
            {
                if (dto.ServerId == 0)
                    dto.ServerId = NextTempId(await db.ClassShifts.Select(x => x.ShiftID).ToListAsync(cancellationToken));
                db.ClassShifts.Add(new LocalClassShift
                {
                    LocalId = dto.LocalId,
                    ShiftID = dto.ServerId,
                    SchoolID = session.SchoolID,
                    ClassID = dto.ClassID,
                    Name = dto.Name,
                    SyncStatus = dto.SyncStatus
                });
            }
            else
            {
                row.Name = dto.Name;
                row.SyncStatus = dto.SyncStatus;
                dto.ServerId = row.ShiftID;
                dto.ClassID = row.ClassID;
            }

            Queue(db, dto.LocalId, EntityTypes.ClassShift, op, ToMutation(dto));
        }

        await db.SaveChangesAsync(cancellationToken);
        await RefreshPendingAsync(cancellationToken);
        StateChanged?.Invoke();
    }

    public async Task DeleteClassPartAsync(string entityType, Guid localId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        ClassPartDto? payload = null;
        var pendingCreate = false;

        if (entityType == EntityTypes.ClassGroup)
        {
            var row = await db.ClassGroups.FirstOrDefaultAsync(x => x.LocalId == localId, cancellationToken);
            if (row is null)
                return;
            pendingCreate = row.SyncStatus == SyncStatus.PendingCreate || row.SubjectGroupID <= 0;
            payload = new ClassPartDto { LocalId = row.LocalId, ServerId = row.SubjectGroupID, ClassID = row.ClassID, Name = row.Name };
            db.ClassGroups.Remove(row);
        }
        else if (entityType == EntityTypes.ClassSection)
        {
            var row = await db.ClassSections.FirstOrDefaultAsync(x => x.LocalId == localId, cancellationToken);
            if (row is null)
                return;
            pendingCreate = row.SyncStatus == SyncStatus.PendingCreate || row.SectionID <= 0;
            payload = new ClassPartDto { LocalId = row.LocalId, ServerId = row.SectionID, ClassID = row.ClassID, Name = row.Name };
            db.ClassSections.Remove(row);
        }
        else
        {
            var row = await db.ClassShifts.FirstOrDefaultAsync(x => x.LocalId == localId, cancellationToken);
            if (row is null)
                return;
            pendingCreate = row.SyncStatus == SyncStatus.PendingCreate || row.ShiftID <= 0;
            payload = new ClassPartDto { LocalId = row.LocalId, ServerId = row.ShiftID, ClassID = row.ClassID, Name = row.Name };
            db.ClassShifts.Remove(row);
        }

        if (pendingCreate)
            DropOutbox(db, localId);
        else
            Queue(db, localId, entityType, SyncOperation.Delete, ToMutation(payload!));

        await db.SaveChangesAsync(cancellationToken);
        await RefreshPendingAsync(cancellationToken);
        StateChanged?.Invoke();
    }

    private async Task PullClassStructureAsync(SessionSnapshot session, string accessToken, CancellationToken cancellationToken)
    {
        var remote = await _api.GetClassStructureAsync(accessToken, cancellationToken);
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);

        await MergeClassesAsync(db, session.SchoolID, remote.Classes, cancellationToken);
        await MergeGroupsAsync(db, session.SchoolID, remote.Groups, cancellationToken);
        await MergeSectionsAsync(db, session.SchoolID, remote.Sections, cancellationToken);
        await MergeShiftsAsync(db, session.SchoolID, remote.Shifts, cancellationToken);
        await MergeJoinsAsync(db, session.SchoolID, remote.Joins, cancellationToken);
        await db.SaveChangesAsync(cancellationToken);
    }

    private async Task ApplyClassStructurePushAsync(
        LocalDbContext db,
        OutboxEntry entry,
        PushItemResult result,
        CancellationToken cancellationToken)
    {
        if (!EntityTypes.IsClassStructure(entry.EntityType))
            return;

        if (entry.EntityType == EntityTypes.Class)
        {
            var row = await db.Classes.FirstOrDefaultAsync(x => x.LocalId == result.LocalId, cancellationToken);
            if (row is null)
                return;
            if (entry.Operation == SyncOperation.Delete)
                return;

            var oldId = row.ClassID;
            if (result.ServerId is int sid && sid > 0 && sid != oldId)
            {
                row.ClassID = sid;
                foreach (var g in db.ClassGroups.Where(x => x.ClassID == oldId))
                    g.ClassID = sid;
                foreach (var s in db.ClassSections.Where(x => x.ClassID == oldId))
                    s.ClassID = sid;
                foreach (var s in db.ClassShifts.Where(x => x.ClassID == oldId))
                    s.ClassID = sid;
                foreach (var j in db.ClassJoins.Where(x => x.ClassID == oldId))
                    j.ClassID = sid;
                foreach (var student in db.Students.Where(x => x.ClassID == oldId))
                    student.ClassID = sid;
                RemapOutboxClassId(db, oldId, sid);
            }

            row.SyncStatus = SyncStatus.Synced;
            return;
        }

        if (entry.EntityType == EntityTypes.ClassGroup)
        {
            var row = await db.ClassGroups.FirstOrDefaultAsync(x => x.LocalId == result.LocalId, cancellationToken);
            if (row is null)
                return;
            var oldId = row.SubjectGroupID;
            if (result.ServerId is int sid && sid > 0)
            {
                row.SubjectGroupID = sid;
                if (sid != oldId)
                {
                    foreach (var j in db.ClassJoins.Where(x => x.SubjectGroupID == oldId))
                        j.SubjectGroupID = sid;
                    RemapOutboxJoinField(db, "SubjectGroupID", oldId, sid);
                }
            }
            row.SyncStatus = SyncStatus.Synced;
        }
        else if (entry.EntityType == EntityTypes.ClassSection)
        {
            var row = await db.ClassSections.FirstOrDefaultAsync(x => x.LocalId == result.LocalId, cancellationToken);
            if (row is null)
                return;
            var oldId = row.SectionID;
            if (result.ServerId is int sid && sid > 0)
            {
                row.SectionID = sid;
                if (sid != oldId)
                {
                    foreach (var j in db.ClassJoins.Where(x => x.SectionID == oldId))
                        j.SectionID = sid;
                    RemapOutboxJoinField(db, "SectionID", oldId, sid);
                }
            }
            row.SyncStatus = SyncStatus.Synced;
        }
        else if (entry.EntityType == EntityTypes.ClassShift)
        {
            var row = await db.ClassShifts.FirstOrDefaultAsync(x => x.LocalId == result.LocalId, cancellationToken);
            if (row is null)
                return;
            var oldId = row.ShiftID;
            if (result.ServerId is int sid && sid > 0)
            {
                row.ShiftID = sid;
                if (sid != oldId)
                {
                    foreach (var j in db.ClassJoins.Where(x => x.ShiftID == oldId))
                        j.ShiftID = sid;
                    RemapOutboxJoinField(db, "ShiftID", oldId, sid);
                }
            }
            row.SyncStatus = SyncStatus.Synced;
        }
        else if (entry.EntityType == EntityTypes.ClassJoin)
        {
            var row = await db.ClassJoins.FirstOrDefaultAsync(x => x.LocalId == result.LocalId, cancellationToken);
            if (row is null)
                return;
            if (result.ServerId is int sid && sid > 0)
                row.JoinID = sid;
            row.SyncStatus = SyncStatus.Synced;
        }
    }

    private static async Task MergeClassesAsync(
        LocalDbContext db,
        int schoolId,
        List<SchoolClassDto> remote,
        CancellationToken cancellationToken)
    {
        var local = await db.Classes.Where(x => x.SchoolID == schoolId).ToListAsync(cancellationToken);
        var remoteIds = remote.Select(x => x.ClassID).ToHashSet();
        foreach (var row in local.Where(x => x.SyncStatus == SyncStatus.Synced && !remoteIds.Contains(x.ClassID)).ToList())
            db.Classes.Remove(row);

        foreach (var item in remote)
        {
            var row = local.FirstOrDefault(x => x.ClassID == item.ClassID)
                      ?? local.FirstOrDefault(x => x.LocalId != Guid.Empty && x.LocalId == item.LocalId && item.LocalId != Guid.Empty);
            if (row is not null && row.SyncStatus is SyncStatus.PendingCreate or SyncStatus.PendingUpdate)
                continue;
            if (row is null)
            {
                db.Classes.Add(new LocalSchoolClass
                {
                    LocalId = item.LocalId == Guid.Empty ? Guid.NewGuid() : item.LocalId,
                    ClassID = item.ClassID,
                    SchoolID = schoolId,
                    Name = item.Name,
                    SortOrder = item.SortOrder,
                    SyncStatus = SyncStatus.Synced
                });
            }
            else
            {
                row.Name = item.Name;
                row.SortOrder = item.SortOrder;
                row.SyncStatus = SyncStatus.Synced;
            }
        }
    }

    private static async Task MergeGroupsAsync(LocalDbContext db, int schoolId, List<ClassPartDto> remote, CancellationToken cancellationToken)
    {
        var local = await db.ClassGroups.Where(x => x.SchoolID == schoolId).ToListAsync(cancellationToken);
        var remoteIds = remote.Select(x => x.ServerId).ToHashSet();
        foreach (var row in local.Where(x => x.SyncStatus == SyncStatus.Synced && !remoteIds.Contains(x.SubjectGroupID)).ToList())
            db.ClassGroups.Remove(row);

        foreach (var item in remote)
        {
            var row = local.FirstOrDefault(x => x.SubjectGroupID == item.ServerId);
            if (row is not null && row.SyncStatus is SyncStatus.PendingCreate or SyncStatus.PendingUpdate)
                continue;
            if (row is null)
            {
                db.ClassGroups.Add(new LocalClassGroup
                {
                    LocalId = Guid.NewGuid(),
                    SubjectGroupID = item.ServerId,
                    SchoolID = schoolId,
                    ClassID = item.ClassID,
                    Name = item.Name,
                    SyncStatus = SyncStatus.Synced
                });
            }
            else
            {
                row.Name = item.Name;
                row.ClassID = item.ClassID;
                row.SyncStatus = SyncStatus.Synced;
            }
        }
    }

    private static async Task MergeSectionsAsync(LocalDbContext db, int schoolId, List<ClassPartDto> remote, CancellationToken cancellationToken)
    {
        var local = await db.ClassSections.Where(x => x.SchoolID == schoolId).ToListAsync(cancellationToken);
        var remoteIds = remote.Select(x => x.ServerId).ToHashSet();
        foreach (var row in local.Where(x => x.SyncStatus == SyncStatus.Synced && !remoteIds.Contains(x.SectionID)).ToList())
            db.ClassSections.Remove(row);

        foreach (var item in remote)
        {
            var row = local.FirstOrDefault(x => x.SectionID == item.ServerId);
            if (row is not null && row.SyncStatus is SyncStatus.PendingCreate or SyncStatus.PendingUpdate)
                continue;
            if (row is null)
            {
                db.ClassSections.Add(new LocalClassSection
                {
                    LocalId = Guid.NewGuid(),
                    SectionID = item.ServerId,
                    SchoolID = schoolId,
                    ClassID = item.ClassID,
                    Name = item.Name,
                    SyncStatus = SyncStatus.Synced
                });
            }
            else
            {
                row.Name = item.Name;
                row.ClassID = item.ClassID;
                row.SyncStatus = SyncStatus.Synced;
            }
        }
    }

    private static async Task MergeShiftsAsync(LocalDbContext db, int schoolId, List<ClassPartDto> remote, CancellationToken cancellationToken)
    {
        var local = await db.ClassShifts.Where(x => x.SchoolID == schoolId).ToListAsync(cancellationToken);
        var remoteIds = remote.Select(x => x.ServerId).ToHashSet();
        foreach (var row in local.Where(x => x.SyncStatus == SyncStatus.Synced && !remoteIds.Contains(x.ShiftID)).ToList())
            db.ClassShifts.Remove(row);

        foreach (var item in remote)
        {
            var row = local.FirstOrDefault(x => x.ShiftID == item.ServerId);
            if (row is not null && row.SyncStatus is SyncStatus.PendingCreate or SyncStatus.PendingUpdate)
                continue;
            if (row is null)
            {
                db.ClassShifts.Add(new LocalClassShift
                {
                    LocalId = Guid.NewGuid(),
                    ShiftID = item.ServerId,
                    SchoolID = schoolId,
                    ClassID = item.ClassID,
                    Name = item.Name,
                    SyncStatus = SyncStatus.Synced
                });
            }
            else
            {
                row.Name = item.Name;
                row.ClassID = item.ClassID;
                row.SyncStatus = SyncStatus.Synced;
            }
        }
    }

    public async Task<bool> TryAddClassJoinAsync(ClassJoinDto dto, SessionSnapshot session, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        if (JoinExists(db, dto))
            return false;

        dto.LocalId = Guid.NewGuid();
        dto.JoinID = NextTempId(await db.ClassJoins.Select(x => x.JoinID).ToListAsync(cancellationToken));
        dto.SyncStatus = SyncStatus.PendingCreate;
        db.ClassJoins.Add(new LocalClassJoin
        {
            LocalId = dto.LocalId,
            JoinID = dto.JoinID,
            SchoolID = session.SchoolID,
            ClassID = dto.ClassID,
            SubjectGroupID = dto.SubjectGroupID,
            SectionID = dto.SectionID,
            ShiftID = dto.ShiftID,
            GroupName = dto.GroupName,
            SectionName = dto.SectionName,
            ShiftName = dto.ShiftName,
            SyncStatus = dto.SyncStatus
        });
        Queue(db, dto.LocalId, EntityTypes.ClassJoin, SyncOperation.Create, dto);
        await db.SaveChangesAsync(cancellationToken);
        await RefreshPendingAsync(cancellationToken);
        StateChanged?.Invoke();
        return true;
    }

    public async Task DeleteClassJoinAsync(Guid localId, CancellationToken cancellationToken = default)
    {
        await using var db = await _dbFactory.CreateDbContextAsync(cancellationToken);
        var row = await db.ClassJoins.FirstOrDefaultAsync(x => x.LocalId == localId, cancellationToken);
        if (row is null)
            return;

        var payload = new ClassJoinDto
        {
            LocalId = row.LocalId,
            JoinID = row.JoinID,
            ClassID = row.ClassID,
            SubjectGroupID = row.SubjectGroupID,
            SectionID = row.SectionID,
            ShiftID = row.ShiftID
        };
        db.ClassJoins.Remove(row);
        if (row.SyncStatus == SyncStatus.PendingCreate || row.JoinID <= 0)
            DropOutbox(db, row.LocalId);
        else
            Queue(db, row.LocalId, EntityTypes.ClassJoin, SyncOperation.Delete, payload);

        await db.SaveChangesAsync(cancellationToken);
        await RefreshPendingAsync(cancellationToken);
        StateChanged?.Invoke();
    }

    private static bool JoinExists(LocalDbContext db, ClassJoinDto dto)
    {
        var q = db.ClassJoins.Where(x => x.ClassID == dto.ClassID);
        if (dto.SubjectGroupID <= 0)
            return q.Any(x => x.SectionID == dto.SectionID && x.ShiftID == dto.ShiftID);
        if (dto.SectionID <= 0)
            return q.Any(x => x.SubjectGroupID == dto.SubjectGroupID && x.ShiftID == dto.ShiftID);
        if (dto.ShiftID <= 0)
            return q.Any(x => x.SubjectGroupID == dto.SubjectGroupID && x.SectionID == dto.SectionID);
        return q.Any(x => x.SubjectGroupID == dto.SubjectGroupID && x.SectionID == dto.SectionID && x.ShiftID == dto.ShiftID);
    }

    private static async Task MergeJoinsAsync(LocalDbContext db, int schoolId, List<ClassJoinDto> remote, CancellationToken cancellationToken)
    {
        var local = await db.ClassJoins.Where(x => x.SchoolID == schoolId).ToListAsync(cancellationToken);
        var remoteIds = remote.Select(x => x.JoinID).ToHashSet();
        foreach (var row in local.Where(x => x.SyncStatus == SyncStatus.Synced && !remoteIds.Contains(x.JoinID)).ToList())
            db.ClassJoins.Remove(row);

        foreach (var item in remote)
        {
            var row = local.FirstOrDefault(x => x.JoinID == item.JoinID);
            if (row is not null && row.SyncStatus is SyncStatus.PendingCreate or SyncStatus.PendingUpdate)
                continue;
            if (row is null)
            {
                db.ClassJoins.Add(new LocalClassJoin
                {
                    LocalId = Guid.NewGuid(),
                    JoinID = item.JoinID,
                    SchoolID = schoolId,
                    ClassID = item.ClassID,
                    SubjectGroupID = item.SubjectGroupID,
                    SectionID = item.SectionID,
                    ShiftID = item.ShiftID,
                    GroupName = item.GroupName,
                    SectionName = item.SectionName,
                    ShiftName = item.ShiftName,
                    SyncStatus = SyncStatus.Synced
                });
            }
            else
            {
                row.ClassID = item.ClassID;
                row.SubjectGroupID = item.SubjectGroupID;
                row.SectionID = item.SectionID;
                row.ShiftID = item.ShiftID;
                row.GroupName = item.GroupName;
                row.SectionName = item.SectionName;
                row.ShiftName = item.ShiftName;
                row.SyncStatus = SyncStatus.Synced;
            }
        }
    }

    private static async Task<List<ClassPartDto>> MapPartsAsync(IQueryable<LocalClassGroup> query, CancellationToken cancellationToken) =>
        await query.OrderBy(x => x.Name).Select(x => new ClassPartDto
        {
            LocalId = x.LocalId,
            ServerId = x.SubjectGroupID,
            ClassID = x.ClassID,
            Name = x.Name,
            SyncStatus = x.SyncStatus
        }).ToListAsync(cancellationToken);

    private static async Task<List<ClassPartDto>> MapPartsAsync(IQueryable<LocalClassSection> query, CancellationToken cancellationToken) =>
        await query.OrderBy(x => x.Name).Select(x => new ClassPartDto
        {
            LocalId = x.LocalId,
            ServerId = x.SectionID,
            ClassID = x.ClassID,
            Name = x.Name,
            SyncStatus = x.SyncStatus
        }).ToListAsync(cancellationToken);

    private static async Task<List<ClassPartDto>> MapPartsAsync(IQueryable<LocalClassShift> query, CancellationToken cancellationToken) =>
        await query.OrderBy(x => x.Name).Select(x => new ClassPartDto
        {
            LocalId = x.LocalId,
            ServerId = x.ShiftID,
            ClassID = x.ClassID,
            Name = x.Name,
            SyncStatus = x.SyncStatus
        }).ToListAsync(cancellationToken);

    private static void Queue(LocalDbContext db, Guid localId, string entityType, SyncOperation operation, ClassMutationDto dto)
    {
        QueueObject(db, localId, entityType, operation, dto);
    }

    private static void Queue(LocalDbContext db, Guid localId, string entityType, SyncOperation operation, ClassJoinDto dto)
    {
        QueueObject(db, localId, entityType, operation, dto);
    }

    private static void QueueObject(LocalDbContext db, Guid localId, string entityType, SyncOperation operation, object dto)
    {
        db.Outbox.Add(new OutboxEntry
        {
            LocalId = localId,
            EntityType = entityType,
            Operation = operation,
            PayloadJson = JsonSerializer.Serialize(dto),
            CreatedUtc = DateTime.UtcNow
        });
    }

    private static void DropOutbox(LocalDbContext db, Guid localId)
    {
        db.Outbox.RemoveRange(db.Outbox.Where(x => x.LocalId == localId));
    }

    private static void RemapOutboxJoinField(LocalDbContext db, string field, int oldId, int newId)
    {
        foreach (var entry in db.Outbox.Where(x => x.EntityType == EntityTypes.ClassJoin).ToList())
        {
            try
            {
                var node = JsonNode.Parse(entry.PayloadJson);
                if (node?[field]?.GetValue<int>() != oldId)
                    continue;
                node[field] = newId;
                entry.PayloadJson = node.ToJsonString();
            }
            catch
            {
            }
        }
    }

    private static void RemapOutboxClassId(LocalDbContext db, int oldId, int newId)
    {
        foreach (var entry in db.Outbox.Where(x => EntityTypes.ClassStructure.Contains(x.EntityType)).ToList())
        {
            try
            {
                var node = JsonNode.Parse(entry.PayloadJson);
                if (node?["ClassID"]?.GetValue<int>() != oldId)
                    continue;
                node["ClassID"] = newId;
                if (entry.EntityType == EntityTypes.Class && node["ServerId"]?.GetValue<int>() == oldId)
                    node["ServerId"] = newId;
                entry.PayloadJson = node.ToJsonString();
            }
            catch
            {
            }
        }
    }

    private static ClassMutationDto ToMutation(SchoolClassDto dto) => new()
    {
        LocalId = dto.LocalId,
        ServerId = dto.ClassID > 0 ? dto.ClassID : 0,
        ClassID = dto.ClassID,
        Name = dto.Name,
        SortOrder = dto.SortOrder
    };

    private static ClassMutationDto ToMutation(ClassPartDto dto) => new()
    {
        LocalId = dto.LocalId,
        ServerId = dto.ServerId > 0 ? dto.ServerId : 0,
        ClassID = dto.ClassID,
        Name = dto.Name
    };

    private static int NextTempId(IReadOnlyList<int> ids)
    {
        var min = ids.Count == 0 ? 0 : ids.Min();
        return Math.Min(min, 0) - 1;
    }
}
