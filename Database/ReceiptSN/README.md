# Money receipt numbers: per-school sequence (never reused)

Files
- `check_duplicates_readonly.sql`: SELECT only. Lists duplicate (SchoolID, MoneyReceipt_SN) in all schools and shows the current `dbo.MoneyReceipt` definition and indexes.
- `receipt_sn_setup.sql`: idempotent and additive. Changes no rows.
  - Creates `dbo.usp_NextMoneyReceiptSN` and `dbo.usp_SkipMoneyReceiptSN`.
  - Creates one `dbo.Seq_MoneyReceipt_<SchoolID>` (CACHE 50) per school, starting at MAX+1.
  - `CREATE OR ALTER`s `dbo.MoneyReceipt`. Parameters and result stay the same, and there is a fallback.
  - Creates `UX_Income_MoneyReceipt_School_SN` (Step 3, before the sequences and the `dbo.MoneyReceipt` switch).
    - No duplicates: plain unique index (new databases, same as `EduHybrid_Schema_Only.sql`).
    - Historical duplicates (live: 6 pairs, largest MoneyReceiptID 1166495): they keep their numbers (parents hold the paper receipts). The index is filtered to `MoneyReceiptID > <largest duplicate MoneyReceiptID>`, so every new receipt is covered.
    - A duplicate above `@ReviewedDuplicateCutoff` (Step 3, set to 1166495) is new: the index is refused and the rows are listed. Everything else is still installed.
    - An existing index (with or without the cutoff) is left unchanged.
    - Adds the non-unique lookup index `IX_Income_MoneyReceipt_School_SN` only when no usable (SchoolID, MoneyReceipt_SN) index exists. A cutoff-filtered unique index cannot serve the per-school MAX/EXISTS lookups, so on live it is created.
  - SchoolID 0 (Authority logins, not a school) gets no sequence. After setup, a collection with SchoolID 0 fails with error 51000 instead of creating an orphan receipt.
  - Ends with a verification SELECT.
- `receipt_sn_rollback.sql`: puts back the original `dbo.MoneyReceipt` and drops both procedures, all sequences and the unique index.
- `test/`: the LocalDB test scripts used to verify all of the above (these need a throwaway database; never run them on Edu). `30_`–`34_` reproduce the live duplicate pattern in `ReceiptSnDupTest`.

Deploy order
1. Publish Hybrid SyncApi (the new code falls back to MAX+1 until the procedure exists). Publishing V2 is optional: the V2 C# is unchanged.
2. Run `check_duplicates_readonly.sql` on Edu. If result set 2 differs from the reviewed list (largest MoneyReceiptID 1166495), review the new rows and adjust `@ReviewedDuplicateCutoff` in Step 3 of the setup.
3. Run `receipt_sn_setup.sql` at a quiet time (the index build locks the table briefly on Standard edition). Check the last two result sets.
4. Only if the setup reported "unique index REFUSED": review the listed rows, set `@ReviewedDuplicateCutoff` (or fix the rows), and run the setup again. Re-running is safe and changes nothing that already exists.

Operations
- After a DB restore, a VM snapshot revert, or any "lost writes" incident, run this before collections resume:
  `EXEC dbo.usp_SkipMoneyReceiptSN @Skip = 100;`
  This moves every school's counter past both its MAX and its last issued number.
- An unexpected SQL Server shutdown by itself needs nothing. Each active school skips up to 50 numbers automatically.
- Institution reset (`sp_ResetInstitutionData`) deletes receipts, but the school's sequence continues from its old number. To restart at 100001, also run `DROP SEQUENCE dbo.Seq_MoneyReceipt_<SchoolID>`.
