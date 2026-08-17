using Sikkhaloy.Shared.Students;
using Sikkhaloy.Shared.Sync;

namespace Sikkhaloy.Shared.Classes;

public sealed class ClassStructureDto
{
    public List<SchoolClassDto> Classes { get; set; } = [];
    public List<ClassPartDto> Groups { get; set; } = [];
    public List<ClassPartDto> Sections { get; set; } = [];
    public List<ClassPartDto> Shifts { get; set; } = [];
    public List<ClassJoinDto> Joins { get; set; } = [];

    public (List<ClassPartDto> Groups, List<ClassPartDto> Sections, List<ClassPartDto> Shifts) AssignedParts(int classId)
    {
        if (classId <= 0)
            return ([], [], []);

        var classGroups = Groups.Where(x => x.ClassID == classId).OrderBy(x => x.Name).ToList();
        var classSections = Sections.Where(x => x.ClassID == classId).OrderBy(x => x.Name).ToList();
        var classShifts = Shifts.Where(x => x.ClassID == classId).OrderBy(x => x.Name).ToList();
        var joins = Joins.Where(x => x.ClassID == classId).ToList();
        if (joins.Count == 0)
            return (classGroups, classSections, classShifts);

        var groupIds = joins.Select(x => x.SubjectGroupID).Where(id => id > 0).ToHashSet();
        var sectionIds = joins.Select(x => x.SectionID).Where(id => id > 0).ToHashSet();
        var shiftIds = joins.Select(x => x.ShiftID).Where(id => id > 0).ToHashSet();
        return (
            groupIds.Count == 0 ? [] : classGroups.Where(x => groupIds.Contains(x.ServerId)).ToList(),
            sectionIds.Count == 0 ? [] : classSections.Where(x => sectionIds.Contains(x.ServerId)).ToList(),
            shiftIds.Count == 0 ? [] : classShifts.Where(x => shiftIds.Contains(x.ServerId)).ToList());
    }
}

public sealed class ClassPartDto
{
    public Guid LocalId { get; set; }
    public int ServerId { get; set; }
    public int ClassID { get; set; }
    public string Name { get; set; } = "";
    public SyncStatus SyncStatus { get; set; }
}

public sealed class ClassMutationDto
{
    public Guid LocalId { get; set; }
    public int ServerId { get; set; }
    public int ClassID { get; set; }
    public string Name { get; set; } = "";
    public int SortOrder { get; set; }
}

public sealed class ClassJoinDto
{
    public Guid LocalId { get; set; }
    public int JoinID { get; set; }
    public int ClassID { get; set; }
    public int SubjectGroupID { get; set; }
    public int SectionID { get; set; }
    public int ShiftID { get; set; }
    public string GroupName { get; set; } = "";
    public string SectionName { get; set; } = "";
    public string ShiftName { get; set; } = "";
    public SyncStatus SyncStatus { get; set; }
}
