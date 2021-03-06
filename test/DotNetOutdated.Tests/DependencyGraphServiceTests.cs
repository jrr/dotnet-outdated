﻿using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using DotNetOutdated.Exceptions;
using DotNetOutdated.Services;
using Moq;
using Xunit;

namespace DotNetOutdated.Tests
{
    public class DependencyGraphServiceTests
    {
        private const string Path = @"c:\path";

        [Fact]
        public void SuccessfulDotNetRunnerExecution_ReturnsDependencyGraph()
        {
            var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>());

            // Arrange
            var dotNetRunner = new Mock<IDotNetRunner>();
            dotNetRunner.Setup(runner => runner.Run(It.IsAny<string>(), It.IsAny<string[]>()))
                .Returns(new RunStatus(string.Empty, string.Empty, 0))
                .Callback((string directory, string[] arguments) =>
                {
                    // Grab the temp filename that was passed... 
                    string tempFileName = arguments[3].Replace("/p:RestoreGraphOutputPath=", string.Empty).Trim('"');

                    // ... and stuff it with our dummy dependency graph
                    mockFileSystem.AddFileFromEmbeddedResource(tempFileName, GetType().Assembly, "DotNetOutdated.Tests.TestData.test.dg");
                });
            
            var graphService = new DependencyGraphService(dotNetRunner.Object, mockFileSystem);
            
            // Act
            var dependencyGraph = graphService.GenerateDependencyGraph(Path);

            // Assert
            Assert.NotNull(dependencyGraph);
        }
        
        [Fact]
        public void UnsuccessfulDotNetRunnerExecution_Throws()
        {
            var mockFileSystem = new MockFileSystem(new Dictionary<string, MockFileData>());

            // Arrange
            var dotNetRunner = new Mock<IDotNetRunner>();
            dotNetRunner.Setup(runner => runner.Run(It.IsAny<string>(), It.IsAny<string[]>()))
                .Returns(new RunStatus(string.Empty, string.Empty, 1));
            
            var graphService = new DependencyGraphService(dotNetRunner.Object, mockFileSystem);
            
            // Assert
            Assert.Throws<CommandValidationException>(() => graphService.GenerateDependencyGraph(Path));
        }

    }
}