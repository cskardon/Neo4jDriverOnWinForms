namespace Neo4jDriverOnWinForms
{
    using System;
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using System.Windows.Forms;
    using Neo4j.Driver;

    public partial class Form1 : Form
    {
        private readonly IDriver _driver;

        public Form1(string uri, string user, string password)
        {
            InitializeComponent();
            _driver = GraphDatabase.Driver(uri, AuthTokens.Basic(user, password));
        }

        //
        // movies METHODS
        //

        /// <summary>
        /// Wrapped in a Task.Run call to work on the STAThread.
        /// </summary>
        /// <returns></returns>
        public Task<List<string>> ReturnMovies_WrappedTask()
        {
            //Wrap here
            var t = Task.Run(async () =>
            {
                var session = _driver.AsyncSession();
                try
                {
                    return await session.ReadTransactionAsync(async tx =>
                    {
                        var result = await tx.RunAsync("MATCH (n:BlockDiagram) RETURN n LIMIT 25");
                        return await result.ToListAsync(r => r[0].As<string>());
                    });
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.ToString());
                }
                finally
                {
                    await session.CloseAsync();
                }

                return new List<string>();
            });

            //return - NB we return the 'Task' not the result.
            return t;
        }

        /// <summary>
        /// This is the original ReturnMovies method
        /// </summary>
        /// <returns></returns>
        public async Task<List<string>> ReturnMovies_Original()
        {
            var session = _driver.AsyncSession();
            try
            {
                return await session.ReadTransactionAsync(async tx =>
                {
                    var result = await tx.RunAsync("MATCH (n:BlockDiagram) RETURN n LIMIT 25");
                    return await result.ToListAsync(r => r[0].As<string>());
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                await session.CloseAsync();
            }

            return new List<string>();
        }

        /// <summary>
        /// This is the most efficient way to get the count of BlockDiagrams, as you'll be minimising Query time and transmission time.
        /// Obviously - if the plan is to use the results later - then this isn't appropriate, but if that would result in another call, this would be best.
        /// </summary>
        /// <returns></returns>
        public async Task<int> ReturnMovies_Fastest()
        {
            var session = _driver.AsyncSession();
            try
            {
                return await session.ReadTransactionAsync(async tx =>
                {

                    var result = await tx.RunAsync("MATCH (n:BlockDiagram) RETURN COUNT(n)");
                    await result.FetchAsync();
                    return result.Current["COUNT(n)"].As<int>();

                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.ToString());
            }
            finally
            {
                await session.CloseAsync();
            }

            return 0;
        }

        //
        // EVENTS HERE!!!
        //


        private async void button2_Click(object sender, EventArgs e)
        {
            var movies = await ReturnMovies_Original();
            MessageBox.Show($"Async Button Event: {movies.Count}");
        }

        private void button1_Click(object sender, EventArgs e)
        {
            var movies = ReturnMovies_WrappedTask();
            movies.Wait();

            MessageBox.Show($"Sync Button Event: {movies.Result.Count}");
        }

        private async void Form1_Load(object sender, EventArgs e)
        {
            var movies = await ReturnMovies_Original();
            MessageBox.Show($"Async Form_Load Event: {movies.Count}");
        }
    }
}