namespace ModernBoxes.Infrastructure.Data
{
    public interface IConfigBackupService
    {
        /// <summary>Silent backup to {AppDir}/.backup/pre-migrate_{timestamp}/. Returns backup dir or null on failure.</summary>
        string? CreatePreMigrateBackup();
    }

}
