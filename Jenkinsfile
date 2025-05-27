// This Jenkinsfile builds a .NET project, runs tests with code coverage,
// and publishes the coverage report using the Cobertura plugin.

pipeline {
    // Define the agent where the pipeline will run.
    // 'any' means it can run on any available agent.
    // IMPORTANT: Ensure your Jenkins agent has the .NET SDK installed.
    // If using Docker agents, you might need something like:
    agent {
        // Using the dotnet SDK image as the agent for the entire pipeline
        docker {
            image 'mcr.microsoft.com/dotnet/sdk:9.0' // Use a specific version or 'latest'
            args '-v $HOME/.nuget:/root/.nuget' // Mount NuGet cache for faster restores
        }
    }

    // Define the stages of the pipeline
    stages {
        stage('Checkout') {
            steps {
                // Clone the source code from the Git repository
                git url: 'https://github.com/SoloveyItstep/Assessment.git',
                    branch: 'master' // Make sure this matches your branch name
            }
        }

        // Skipping explicit Restore, Build stages as dotnet test/build can do it.
        // Added them back based on user's log structure, but marked as --no-restore/--no-build

        stage('Restore Dependencies') {
            steps {
                echo 'Restoring NuGet packages...'
                sh 'dotnet restore Assessment.sln'
            }
        }

        stage('Build') {
            steps {
                echo 'Building solution...'
                // Use --no-restore as restore was done in the previous stage
                sh 'dotnet build Assessment.sln --configuration Release --no-restore'
            }
        }

        stage('Test and Collect Coverage') {
            steps {
                echo 'Running .NET tests and collecting coverage...'
                // Your existing command from the log, corrected results-directory to be relative
                // /p:CoverletOutput sets the exact path, which is now relative to workspace
                sh 'dotnet test Session.UnitTests/Session.UnitTests.csproj --configuration Release --no-build /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura /p:CoverletOutput=${WORKSPACE}/TestResults/coverage.xml --results-directory ${WORKSPACE}/TestResults'
                // Note: Using ${WORKSPACE} is generally reliable in Jenkins pipelines to get the workspace root path.
                // Alternatively, using just 'TestResults/coverage.xml' for CoverletOutput and 'TestResults' for results-directory might also work and is cleaner. Let's try the ${WORKSPACE} version as it mirrors your log command structure.
            }
        }

        // Based on your log, there might be a post-stage action happening implicitly
        // after "Test and Collect Coverage" stage but before the next stage.
        // This post action seems to contain the failing findFiles.
        // I cannot directly fix an implicit post action from a log.
        // However, I will provide a sample 'post' block structure below
        // showing how to fix the findFiles if it's located there.
        // You might need to integrate this correction into your actual Jenkinsfile structure
        // if the findFiles call is indeed in a post block.

        stage('Publish Coverage Report') {
            steps {
                echo 'Publishing Coverage Report...'
                // Publish the code coverage report using the Cobertura plugin
                // The report was generated at TestResults/coverage.xml by the previous step
                // Ensure the "Cobertura coverage plugin" is installed in Jenkins
                cobertura autoUpdateHealth: false,
                          autoUpdateStability: false,
                          coberturaReportFile: 'TestResults/coverage.xml', // Corrected path based on your dotnet test command
                          failUnhealthy: false, // Set to true to fail build on unhealthy coverage
                          failUnstable: false,  // Set to true to fail build on unstable coverage
                          lineCoverageTargets: '0, 0, 0', // Example: '70, 80, 90' for thresholds (unhealthy, unstable, healthy)
                          onlyStable: false,
                          sourceEncoding: 'ASCII', // Or your project's encoding
                          stabilityTargets: '0, 0, 0' // Example: '70, 80, 90' for thresholds
            }
        }

        // Add other stages like Publish Application, Build Docker Image, etc. as needed
        // ... (Your other stages here) ...

    }

    // Optional: Add post-build actions (e.g., clean workspace, report generation)
    // This is also likely where your failing findFiles call is located based on the log.
    post {
        always {
            echo 'Cleaning up workspace...'
            cleanWs() // Clean up the workspace after each build

            // --- POTENTIAL LOCATION OF THE FAILING findFiles ---
            // If your Jenkinsfile has a 'post' section similar to this
            // within or after the 'Test and Collect Coverage' stage, or a global 'post'
            // block, this is where the findFiles error likely occurred.
            // Fix the path here:
            // Example of fixing findFiles for .trx files (if needed):
            // try {
            //     echo 'Listing contents of TestResults directory for conversion:'
            //     // CORRECTED PATH: Use a relative Ant GLOB pattern
            //     def trxFiles = findFiles(glob: 'TestResults/**/*.trx') // Assuming .trx files are in TestResults or subdirs
            //     echo "Found ${trxFiles.size()} TRX files."
            //     // Add steps here to process TRX files, e.g., convert to JUnit XML
            // } catch (IOException e) {
            //      echo "Error finding TRX files: ${e.getMessage()}"
            //      // Depending on whether finding TRX files is critical, you might want to
            //      // mark the build as failed here: error("Failed to find TRX files")
            // }
            // --- END OF POTENTIAL findFiles LOCATION ---

            // You can add more post-build actions here
             script {
                 // Example of sending a notification based on build status
                 if (currentBuild.currentResult == 'SUCCESS') {
                     echo "Pipeline finished successfully for branch ${env.BRANCH_NAME} and environment ${env.DEPLOY_ENVIRONMENT}!"
                 } else {
                     echo "Pipeline failed for branch ${env.BRANCH_NAME} and environment ${env.DEPLOY_ENVIRONMENT}!"
                 }
             }
        }
        // You can add other post conditions like 'success', 'failure', 'unstable', etc.
        // failure {
        //    echo 'Pipeline failed!'
        //    // Add failure-specific actions, e.g., send failure notification
        // }
    }
}
