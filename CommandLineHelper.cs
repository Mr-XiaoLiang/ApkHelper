using System;
using System.Diagnostics;
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
        private readonly string _commandTarget;

        private CommandLineHelper(string target, string cmd)
        {
            _commandInfo = cmd;
            _commandTarget = target;
        }

        public static CommandLineHelper Create(string target, params string[] commands)
        {
            var commandBuilder = new StringBuilder();
            foreach (var item in commands)
            {
                commandBuilder.Append(item);
                commandBuilder.Append(' ');
            }

            var cmdFinal = commandBuilder.ToString();
            if (target.Length <= 0)
            {
                return null;
            }
            var task = new CommandLineHelper(target, cmdFinal);
            return task;
        }

        public CommandLineHelper Send()
        {
            var startInfo = new ProcessStartInfo()
            {
                Arguments = _commandInfo,
                FileName = _commandTarget,
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                RedirectStandardInput = true,
                WindowStyle = ProcessWindowStyle.Hidden,
                CreateNoWindow = true,
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
            var data = e.Data;
            if (data == null)
            {
                return;
            }
            CmdError?.Invoke(data);
        }

        private void OnCmdOutput(object sender, DataReceivedEventArgs e)
        {
            var data = e.Data;
            if (data == null)
            {
                return;
            }
            CmdOutput?.Invoke(data);
        }

        private void OnCmdExited(object sender, EventArgs e)
        {
            CmdExit?.Invoke();
        }
    }
}