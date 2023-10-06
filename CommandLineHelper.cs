using System;
using System.Diagnostics;
using System.IO;
using System.Text;

namespace ApkHelper
{
    public class CommandLineHelper
    {


        public delegate void CmdExitCallback();
        public delegate void CmdOutputCallback(string value);
        public delegate void CmdErrorCallback(string value);

        private event CmdExitCallback CmdExit;
        private event CmdOutputCallback CmdOutput;
        private event CmdErrorCallback CmdError;

        private Process _process = new();

        private readonly string _commandInfo;

        private CommandLineHelper(string cmd) {
            this._commandInfo = cmd;
        }

        public static CommandLineHelper Create(params string[] commands)
        {
            var commandBuilder = new StringBuilder();
            foreach (var item in commands)
            {
                commandBuilder.Append(item);
                commandBuilder.Append(' ');
            }
            var cmdFinla = commandBuilder.ToString();
            if (cmdFinla.Length > 0 )
            {
                var task = new CommandLineHelper(cmdFinla);
                return task;
            } 
            else
            {
                return null;
            }
        }

        public CommandLineHelper Send()
        {
            var cmd = _commandInfo;
            var path = Path.Combine(Environment.SystemDirectory, @"cmd.exe");

            if (!cmd.StartsWith(@"/"))
            {
                cmd = @"/c " + cmd;
            }

            var startInfo = new ProcessStartInfo()
            {
                Arguments = cmd,
                FileName = path,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            _process.StartInfo = startInfo;
            _process.EnableRaisingEvents = true;
            _process.ErrorDataReceived += OnCmdError;
            _process.OutputDataReceived += OnCmdOutput;
            _process.Exited += OnCmdExited;
            _process.Start();
            _process.BeginErrorReadLine();
            _process.BeginOutputReadLine();
            return this;
        }

        public CommandLineHelper Write(string data)
        {
            _process.StandardInput.WriteLine(data);
            return this;
        }

        public CommandLineHelper OnError(CmdErrorCallback callback)
        {
            CmdError = callback;
            return this;
        }

        public CommandLineHelper OnExit(CmdExitCallback callback)
        {
            CmdExit = callback;
            return this;
        }

        public CommandLineHelper OnOutput(CmdOutputCallback callback)
        {
            CmdOutput = callback;
            return this;
        }

        private void OnCmdError(object sender, DataReceivedEventArgs e)
        {
            CmdError?.Invoke(e.Data);
        }

        private void OnCmdOutput(object sender, DataReceivedEventArgs e)
        {
            CmdOutput?.Invoke(e.Data);
        }

        private void OnCmdExited(object sender, EventArgs e)
        {
            CmdExit?.Invoke();
        }

    }

}
