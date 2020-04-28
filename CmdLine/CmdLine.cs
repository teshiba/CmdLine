using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;

namespace CmdLineProcess
{
    /// <summary>
    /// command line process executing class.
    /// </summary>
    public class CmdLine : Process
    {
        private readonly Action<string> onReceive;
        private readonly Action<int> onExit;
        private TaskCompletionSource<int> processExitCode;

        /// <summary>
        /// Initializes a new instance of the <see cref="CmdLine"/> class.
        /// </summary>
        /// <param name="commandName">Command file name.</param>
        /// <param name="workPath">Work directory.</param>
        /// <param name="onReceive">Action for recieving output.</param>
        /// <param name="onExit">Action for after exiting process.</param>
        public CmdLine(string commandName, string workPath, Action<string> onReceive, Action<int> onExit)
        {
            OutputDataReceived += Process_OutputDataReceived;
            ErrorDataReceived += Process_OutputDataReceived;
            Exited += Process_Exited;
            EnableRaisingEvents = true;

            StartInfo = new ProcessStartInfo() {
                FileName = commandName,
                CreateNoWindow = true,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                WorkingDirectory = workPath,
            };

            this.onReceive = onReceive;
            this.onExit = onExit;
        }

        /// <summary>
        /// Start process.
        /// </summary>
        /// <param name="args">arguments.</param>
        public new void Start(string args)
        {
            StartInfo.Arguments = args;

            if (Directory.Exists(StartInfo.WorkingDirectory)) {
                base.Start();
                BeginOutputReadLine();
                BeginErrorReadLine();
            }
        }

        /// <summary>
        /// Start process.
        /// </summary>
        public new void Start()
        {
            Start(string.Empty);
        }

        /// <summary>
        /// Start process async.
        /// </summary>
        /// <param name="args">arguments.</param>
        /// <returns>A <see cref="Task"/> representing the asynchronous operation.</returns>
        public virtual async Task<int> StartAsync(string args)
        {
            Start(args);

            // Wait Process_Exited is called.
            processExitCode = new TaskCompletionSource<int>();
            return await processExitCode.Task.ConfigureAwait(false);
        }

        /// <summary>
        /// Occurs when a process exits.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An object that contains no event data.</param>
        private void Process_Exited(object sender, EventArgs e)
        {
            // Wait output all buffer data.
            if (!HasExited) {
                WaitForExit();
            }

            try {
                onExit?.Invoke(ExitCode);
            } catch (Exception ex) {
                Console.WriteLine(ex.ToString());
                throw;
            }

            // Throws "ExitCode" to the waiting task in StartAsync .
            processExitCode?.SetResult(ExitCode);
        }

        /// <summary>
        /// Occurs when output data received.
        /// </summary>
        /// <param name="sender">The source of the event.</param>
        /// <param name="e">An object that contains no event data.</param>
        private void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            try {
                onReceive?.Invoke(e.Data);
            } catch (Exception ex) {
                Console.WriteLine(ex.ToString());
                throw;
            }
        }
    }
}