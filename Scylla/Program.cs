namespace Scylla
{
    internal static class Program
    {
        /// <summary>
        /// Hlavn� vstupn� bod aplikace.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //Update update = new Update();
            //update.AutoUpdate();
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.SetCompatibleTextRenderingDefault(false);
            Application.SetUnhandledExceptionMode(UnhandledExceptionMode.ThrowException);
            Application.Run(new Login());
        }
    }
}
