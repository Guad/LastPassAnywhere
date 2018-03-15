namespace LastPassAnywhere
{
    public static class Clipboard
    {
        public static bool TryCopy(string val)
        {
            if (OperatingSystem.IsWindows)
            {
                $"echo {val.Replace("&", "^^^&").Replace("|", "^^^|")} | clip".Bat();
            }
            else if (OperatingSystem.IsMac)
            {
                $" echo \"{val}\" | pbcopy".Bash();
            }
            else if (OperatingSystem.IsLinux)
            {
                $" echo \"{val}\" | xclip -selection c".Bash();
            }
            else return false;
            return true;
        }
    }
}