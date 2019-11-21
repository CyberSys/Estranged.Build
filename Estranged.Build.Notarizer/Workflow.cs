﻿using System.Linq;
using System.Threading.Tasks;

namespace Estranged.Build.Notarizer
{
    internal sealed class Workflow
    {
        private readonly ExecutableFinder executableFinder;
        private readonly ExecutableSigner executableSigner;
        private readonly ExecutableZipBuilder executableZipBuilder;
        private readonly ExecutableNotarizer executableNotarizer;

        public Workflow(ExecutableFinder executableFinder, ExecutableSigner executableSigner, ExecutableZipBuilder executableZipBuilder, ExecutableNotarizer executableNotarizer)
        {
            this.executableFinder = executableFinder;
            this.executableSigner = executableSigner;
            this.executableZipBuilder = executableZipBuilder;
            this.executableNotarizer = executableNotarizer;
        }

        public async Task Run(NotarizerConfiguration configuration)
        {
            var executables = executableFinder.FindExecutables(configuration.AppDirectory).ToArray();

            foreach (var executable in executables)
            {
                if (configuration.EntitlementsMap.TryGetValue(executable.Name, out string[] entitlements))
                {
                    executableSigner.SignExecutable(configuration.CertificateId, executable, entitlements);
                }
                else
                {
                    executableSigner.SignExecutable(configuration.CertificateId, executable, new string[0]);
                }
            }

            var zipFile = executableZipBuilder.BuildZipFile(executables);

            await executableNotarizer.NotarizeExecutables(zipFile, configuration.AppDirectory, configuration.DeveloperUsername, configuration.DeveloperPassword);
        }
    }
}