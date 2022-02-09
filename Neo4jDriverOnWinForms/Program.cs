namespace Neo4jDriverOnWinForms
{
    using System;
    using System.Windows.Forms;

    internal static class Program
    {
        /// <summary>
        ///     The main entry point for the application.
        /// </summary>
        [STAThread]
        private static void Main()
        {
            Application.SetHighDpiMode(HighDpiMode.SystemAware);
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            var form1 = new Form1("neo4j://localhost:7687", "neo4j", "neo");
            
            var movies = form1.ReturnMovies_WrappedTask();
            movies.Wait();
            MessageBox.Show($"Main() Method: {movies.Result.Count}");

            Application.Run(form1);
        }
    }
}