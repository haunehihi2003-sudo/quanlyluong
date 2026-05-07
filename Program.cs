using QuanLyLuong.Forms;
using QuanLyLuong.Services;

namespace QuanLyLuong;

static class Program
{
    [STAThread]
    static void Main()
    {
        ApplicationConfiguration.Initialize();
        DataService.EnsureDefaultUsers();
        var login = new LoginForm();
        if (login.ShowDialog() != DialogResult.OK) return;
        Application.Run(new MainForm(login.LoggedInUser!));
    }
}
