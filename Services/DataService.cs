using Newtonsoft.Json;
using QuanLyLuong.Models;
using System.Security.Cryptography;
using System.Text;

namespace QuanLyLuong.Services;

public static class DataService
{
    private static readonly string DataFolder = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData), "QuanLyLuong");

    private static string F(string name) => Path.Combine(DataFolder, name);

    static DataService() => Directory.CreateDirectory(DataFolder);

    // ── Generic helpers ──────────────────────────────────────────────────────
    private static List<T> Load<T>(string file)
    {
        var p = F(file);
        if (!File.Exists(p)) return new();
        return JsonConvert.DeserializeObject<List<T>>(File.ReadAllText(p)) ?? new();
    }
    private static void Save<T>(string file, List<T> list) =>
        File.WriteAllText(F(file), JsonConvert.SerializeObject(list, Formatting.Indented));

    // ── Employees ─────────────────────────────────────────────────────────────
    public static List<Employee> GetEmployees() => Load<Employee>("nhanvien.json");
    public static void SaveEmployees(List<Employee> l) => Save("nhanvien.json", l);
    public static void AddEmployee(Employee e) { var l = GetEmployees(); l.Add(e); SaveEmployees(l); }
    public static void UpdateEmployee(Employee e) { var l = GetEmployees(); var i = l.FindIndex(x => x.Id == e.Id); if (i >= 0) l[i] = e; SaveEmployees(l); }
    public static void DeleteEmployee(string id) { var l = GetEmployees(); l.RemoveAll(e => e.Id == id); SaveEmployees(l); }
    public static Employee? GetEmployee(string id) => GetEmployees().FirstOrDefault(e => e.Id == id);

    // ── Salary Records ────────────────────────────────────────────────────────
    public static List<SalaryRecord> GetSalaryRecords() => Load<SalaryRecord>("luong.json");
    public static void SaveSalaryRecords(List<SalaryRecord> l) => Save("luong.json", l);
    public static List<SalaryRecord> GetSalaryByMonth(int t, int n) => GetSalaryRecords().Where(r => r.Thang == t && r.Nam == n).ToList();
    public static List<SalaryRecord> GetSalaryByEmployee(string empId) => GetSalaryRecords().Where(r => r.EmployeeId == empId).OrderByDescending(r => r.Nam).ThenByDescending(r => r.Thang).ToList();
    public static void UpsertSalaryRecord(SalaryRecord r)
    {
        var l = GetSalaryRecords();
        l.RemoveAll(x => x.EmployeeId == r.EmployeeId && x.Thang == r.Thang && x.Nam == r.Nam);
        l.Add(r);
        SaveSalaryRecords(l);
    }
    public static void DeleteSalaryRecord(string id) { var l = GetSalaryRecords(); l.RemoveAll(r => r.Id == id); SaveSalaryRecords(l); }

    // ── Attendance ────────────────────────────────────────────────────────────
    public static List<AttendanceRecord> GetAttendance() => Load<AttendanceRecord>("chamcong.json");
    public static void SaveAttendance(List<AttendanceRecord> l) => Save("chamcong.json", l);
    public static List<AttendanceRecord> GetAttendanceByMonth(int t, int n) => GetAttendance().Where(r => r.Thang == t && r.Nam == n).ToList();
    public static AttendanceRecord? GetAttendance(string empId, int t, int n) => GetAttendance().FirstOrDefault(r => r.EmployeeId == empId && r.Thang == t && r.Nam == n);
    public static void UpsertAttendance(AttendanceRecord r)
    {
        var l = GetAttendance();
        l.RemoveAll(x => x.EmployeeId == r.EmployeeId && x.Thang == r.Thang && x.Nam == r.Nam);
        l.Add(r);
        SaveAttendance(l);
    }

    // ── Leave Records ─────────────────────────────────────────────────────────
    public static List<LeaveRecord> GetLeaveRecords() => Load<LeaveRecord>("nghiphep.json");
    public static void SaveLeaveRecords(List<LeaveRecord> l) => Save("nghiphep.json", l);
    public static void AddLeaveRecord(LeaveRecord r) { var l = GetLeaveRecords(); l.Add(r); SaveLeaveRecords(l); }
    public static void UpdateLeaveRecord(LeaveRecord r) { var l = GetLeaveRecords(); var i = l.FindIndex(x => x.Id == r.Id); if (i >= 0) l[i] = r; SaveLeaveRecords(l); }
    public static void DeleteLeaveRecord(string id) { var l = GetLeaveRecords(); l.RemoveAll(r => r.Id == id); SaveLeaveRecords(l); }

    // ── Bonus/Penalty ─────────────────────────────────────────────────────────
    public static List<BonusPenalty> GetBonusPenalties() => Load<BonusPenalty>("thuongphat.json");
    public static void SaveBonusPenalties(List<BonusPenalty> l) => Save("thuongphat.json", l);
    public static void AddBonusPenalty(BonusPenalty b) { var l = GetBonusPenalties(); l.Add(b); SaveBonusPenalties(l); }
    public static void DeleteBonusPenalty(string id) { var l = GetBonusPenalties(); l.RemoveAll(b => b.Id == id); SaveBonusPenalties(l); }
    public static List<BonusPenalty> GetBonusPenaltiesByMonth(int t, int n) => GetBonusPenalties().Where(b => b.Thang == t && b.Nam == n).ToList();

    // ── Users ─────────────────────────────────────────────────────────────────
    public static List<UserAccount> GetUsers() => Load<UserAccount>("users.json");
    public static void SaveUsers(List<UserAccount> l) => Save("users.json", l);
    public static void EnsureDefaultUsers()
    {
        var users = GetUsers();
        if (users.Count == 0)
        {
            users.Add(new UserAccount { TenDangNhap = "admin", MatKhauHash = Hash("admin123"), HoTen = "Quản Trị Viên", VaiTro = "Admin" });
            users.Add(new UserAccount { TenDangNhap = "ketoan", MatKhauHash = Hash("ketoan123"), HoTen = "Kế Toán", VaiTro = "Kế toán" });
            SaveUsers(users);
        }
    }
    public static UserAccount? Authenticate(string user, string pass) =>
        GetUsers().FirstOrDefault(u => u.TenDangNhap == user && u.MatKhauHash == Hash(pass) && u.ConHieuLuc);
    public static void AddUser(UserAccount u) { var l = GetUsers(); l.Add(u); SaveUsers(l); }
    public static void DeleteUser(string id) { var l = GetUsers(); l.RemoveAll(u => u.Id == id); SaveUsers(l); }
    public static void UpdateUser(UserAccount u) { var l = GetUsers(); var i = l.FindIndex(x => x.Id == u.Id); if (i >= 0) l[i] = u; SaveUsers(l); }
    public static string Hash(string s)
    {
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(s));
        return Convert.ToHexString(bytes);
    }

    // ── Reports ───────────────────────────────────────────────────────────────
    public static string ExportCsv(int t, int n)
    {
        var records = GetSalaryByMonth(t, n);
        var desktop = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
        var path = Path.Combine(desktop, $"BangLuong_{t:D2}_{n}.csv");
        var lines = new List<string> { "Ma NV,Ho Ten,Phong Ban,Luong CB,Phu Cap,Ngay Lam,Gio OT,Tien OT,Thuong,Phat,BHXH,Thue TNCN,KT Khac,Thuc Nhan" };
        foreach (var r in records)
            lines.Add($"{r.MaNhanVien},{r.HoTen},{r.PhongBan},{r.LuongCoBan},{r.PhuCap},{r.SoNgayLam},{r.GioLamThem},{r.TienLamThem},{r.ThuongKhac},{r.PhatKhac},{r.BaoHiemXaHoi},{r.ThueTNCN},{r.KhauTruKhac},{r.LuongThucNhan}");
        File.WriteAllLines(path, lines, Encoding.UTF8);
        return path;
    }

    public static string DataFolderPath => DataFolder;
}
