using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.IO;
using Microsoft.Build.Construction;
using Microsoft.Build.Evaluation;
using Microsoft.Build.Execution;
using Microsoft.Build.Framework;

namespace Treefrog
{
    public class ContentBuilder : IDisposable
    {
        private const string _xnaVersion = ", Version=4.0.0.0, PublicKeyToken=842cf8be1de50553";

        private static string[] _pipelineAssemblies =
        {
            "Microsoft.Xna.Framework.Content.Pipeline.TextureImporter" + _xnaVersion,
            Path.Combine(Environment.CurrentDirectory, "Treefrog.Pipeline.dll"),
        };

        private Project _buildProject;
        private ProjectRootElement _rootElement;
        private BuildParameters _buildParameters;
        private List<ProjectItem> _projectItems = new List<ProjectItem>();
        private ErrorLogger _errorLogger;

        private string _baseDirectory;      // Temp/Treefrog.Content/
        private string _processDirectory;   // Temp/Treefrog.Content/<process_id>/
        private string _buildDirectory;     // Temp/Treefrog.Content/<process_id>/<id>/ 

        private static int _directorySalt;

        private bool _disposed;

        public string OutputDirectory       // Temp/Treefrog.Content/<process_id>/<id>/bin/Content/
        {
            get { return Path.Combine(_buildDirectory, "bin/Content"); }
        }

        public ContentBuilder ()
        {
            CreateTempDirectory();
            CreateBuildProject();
        }

        ~ContentBuilder ()
        {
            Dispose(false);
        }

        public void Dispose ()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose (bool disposing)
        {
            if (!_disposed) {
                _disposed = true;
                DeleteTempDirectory();
            }
        }

        private void CreateBuildProject ()
        {
            string projectPath = Path.Combine(_buildDirectory, "content.contentproj");
            string outputPath = Path.Combine(_buildDirectory, "bin");

            _rootElement = ProjectRootElement.Create(projectPath);

            _rootElement.AddImport("$(MSBuildExtensionsPath)\\Microsoft\\XNA Game Studio\\v4.0\\Microsoft.Xna.GameStudio.ContentPipeline.targets");

            _buildProject = new Project(_rootElement);
            _buildProject.SetProperty("XnaPlatform", "Windows");
            _buildProject.SetProperty("XnaProfile", "Reach");
            _buildProject.SetProperty("XnaFrameworkVersion", "v4.0");
            _buildProject.SetProperty("Configuration", "Release");
            _buildProject.SetProperty("OutputPath", outputPath);

            foreach (string assm in _pipelineAssemblies) {
                _buildProject.AddItem("Reference", assm);
            }

            _errorLogger = new ErrorLogger();

            _buildParameters = new BuildParameters(ProjectCollection.GlobalProjectCollection);
            _buildParameters.Loggers = new ILogger[] { _errorLogger };
        }

        public void Add (string filename, string name, string importer, string processor)
        {
            ProjectItem item = _buildProject.AddItem("Compile", filename)[0];

            item.SetMetadataValue("Link", Path.GetFileName(filename));
            item.SetMetadataValue("Name", name);

            if (!string.IsNullOrEmpty(importer)) {
                item.SetMetadataValue("Importer", importer);
            }

            if (!string.IsNullOrEmpty(processor)) {
                item.SetMetadataValue("Processor", processor);
            }

            _projectItems.Add(item);
        }

        public void Clear ()
        {
            _buildProject.RemoveItems(_projectItems);
            _projectItems.Clear();
        }

        public string Build ()
        {
            _errorLogger.Errors.Clear();

            BuildManager.DefaultBuildManager.BeginBuild(_buildParameters);

            BuildRequestData request = new BuildRequestData(_buildProject.CreateProjectInstance(), new string[0]);
            BuildSubmission submission = BuildManager.DefaultBuildManager.PendBuildRequest(request);

            submission.ExecuteAsync(null, null);
            submission.WaitHandle.WaitOne();

            BuildManager.DefaultBuildManager.EndBuild();

            if (submission.BuildResult.OverallResult == BuildResultCode.Failure) {
                return string.Join("\n", _errorLogger.Errors.ToArray());
            }

            return null;
        }

        private void CreateTempDirectory ()
        {
            _baseDirectory = Path.Combine(Path.GetTempPath(), GetType().FullName);

            int procId = Process.GetCurrentProcess().Id;

            _processDirectory = Path.Combine(_baseDirectory, procId.ToString());

            _directorySalt++;

            _buildDirectory = Path.Combine(_processDirectory, _directorySalt.ToString());

            Directory.CreateDirectory(_buildDirectory);

            PurgeStaleTempDirectories();
        }

        private void DeleteTempDirectory ()
        {
            Directory.Delete(_buildDirectory, true);

            if (Directory.GetDirectories(_processDirectory).Length == 0) {
                Directory.Delete(_processDirectory);

                if (Directory.GetDirectories(_baseDirectory).Length == 0) {
                    Directory.Delete(_baseDirectory);
                }
            }
        }

        private void PurgeStaleTempDirectories ()
        {
            foreach (string dir in Directory.GetDirectories(_baseDirectory)) {
                int procId;
                if (int.TryParse(Path.GetFileName(dir), out procId)) {
                    try {
                        Process.GetProcessById(procId);
                    }
                    catch (ArgumentException) {
                        Directory.Delete(dir, true);
                    }
                }
            }
        }
    }

    public class ErrorLogger : ILogger
    {
        private List<string> _errors = new List<string>();

        public ErrorLogger ()
        {
            Verbosity = LoggerVerbosity.Normal;
        }

        public List<string> Errors
        {
            get { return _errors; }
        }

        public void Initialize (IEventSource eventSource)
        {
            if (eventSource != null) {
                eventSource.ErrorRaised += ErrorRaised;
            }
        }

        public void Shutdown ()
        {
        }

        private void ErrorRaised (object sender, BuildErrorEventArgs e)
        {
            _errors.Add(e.Message);
        }

        #region ILogger Members

        public string Parameters { get; set; }

        public LoggerVerbosity Verbosity { get; set; }

        #endregion
    }
}
