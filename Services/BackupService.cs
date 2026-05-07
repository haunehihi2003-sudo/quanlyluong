namespace QuanLyLuong.Services;

public static class BackupService
{
    public static string Backup(string destinationFolder)
    {
        var src = DataService.DataFolderPath;
        var timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
        var dest = Path.Combine(destinationFolder, $"QuanLyLuong_Backup_{timestamp}");
        Directory.CreateDirectory(dest);
        foreach (var file in Directory.GetFiles(src, "*.json"))
            File.Copy(file, Path.Combine(dest, Path.GetFileName(file)), true);
        return dest;
    }

    public static (int restored, List<string> errors) Restore(string backupFolder)
    {
        var dest = DataService.DataFolderPath;
        var errors = new List<string>();
        int count = 0;
        foreach (var file in Directory.GetFiles(backupFolder, "*.json"))
        {
            try
            {
                File.Copy(file, Path.Combine(dest, Path.GetFileName(file)), true);
                count++;
            }
            catch (Exception ex)
            {
                errors.Add($"{Path.GetFileName(file)}: {ex.Message}");
            }
        }
        return (count, errors);
    }

    public static List<(string Name, string Path, DateTime Date, long SizeKb)> ListBackups(string folder)
    {
        if (!Directory.Exists(folder)) return new();
        return Directory.GetDirectories(folder, "QuanLyLuong_Backup_*")
            .Select(d => new DirectoryInfo(d))
            .OrderByDescending(d => d.CreationTime)
            .Select(d => (d.Name, d.FullName, d.CreationTime,
                d.GetFiles("*.json").Sum(f => f.Length) / 1024))
            .ToList();
    }
}
