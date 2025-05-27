pipeline {
    agent {
        docker {
            image 'mcr.microsoft.com/dotnet/sdk:9.0' // Залишаємо .NET 9.0
            args '-v $HOME/.nuget:/root/.nuget'
        }
    }

    environment {
        BRANCH_NAME = "${env.GIT_BRANCH_NAME ?: 'master'}"
        DEPLOY_ENV = "Production"
        ASPNETCORE_ENVIRONMENT = "Production"
        IMAGE_TAG_SHORT = sh(returnStdout: true, script: 'git rev-parse --short HEAD').trim()
        IMAGE_TAGS = "sessionmvc:latest, sessionmvc:${env.IMAGE_TAG_SHORT}, sessionmvc:${env.DEPLOY_ENV}-${env.IMAGE_TAG_SHORT}"
    }

    stages {
        stage('Initialize and Display Environment') {
            steps {
                script {
                    echo "Current Git branch (from env.BRANCH_NAME via env.GIT_BRANCH_NAME): ${env.BRANCH_NAME}"
                    echo "Deployment Environment: ${env.DEPLOY_ENV}"
                    echo "ASPNETCORE_ENVIRONMENT for application: ${env.ASPNETCORE_ENVIRONMENT}"
                    echo "Image tags set to: ${env.IMAGE_TAGS}"
                }
            }
        }

        stage('Install Tools') {
            steps {
                echo "Installing trx2junit global tool..."
                sh 'dotnet tool install -g trx2junit'
                // Optional: Add a note to ensure plugin is installed
                echo "Please ensure 'Pipeline Utility Steps' plugin is installed in Jenkins for 'findFiles' to work."
            }
        }

        stage('Restore') {
            steps {
                sh 'dotnet restore Assessment.sln'
            }
        }

        stage('Build') {
            steps {
                echo "Building solution (Solution: Assessment.sln)..."
                sh 'dotnet build Assessment.sln --no-restore --configuration Release'
            }
        }

        stage('Test and Collect Coverage') {
            steps {
                echo "Running .NET tests and collecting coverage (Project: Session.UnitTests.csproj)..."
                // Запускаємо тести на конкретному тестовому проекті.
                // CoverletOutput вказує на кореневу TestResults директорію у WORKSPACE.
                // --results-directory: вказуємо, куди VSTestRunner буде скидати свої TRX-файли.
                // Зауваження: VSTestRunner створює піддиректорію з GUID всередині вказаної.
                sh 'dotnet test Session.UnitTests/Session.UnitTests.csproj ' +
                   '--configuration Release ' +
                   '--no-build ' +
                   '/p:CollectCoverage=true ' +
                   '/p:CoverletOutputFormat=cobertura ' +
                   '/p:CoverletOutput="${WORKSPACE}/TestResults/coverage.xml" ' + // Це для звіту Coverlet
                   '--results-directory "${WORKSPACE}/TestResults"' // Це для VSTest (TRX)
            }
            post {
                always {
                    script {
                        echo "Listing contents of TestResults directory for conversion:"
                        sh "ls -R ${WORKSPACE}/TestResults" // Перевіряємо вміст TestResults

                        // Знаходимо TRX файл. Він повинен бути у піддиректорії з GUID.
                        // findFiles шукає рекурсивно за маскою.
                        def trxFiles = findFiles(glob: "${WORKSPACE}/TestResults/**/*.trx")

                        if (trxFiles.length == 0) {
                            error "TRX test results file not found in ${WORKSPACE}/TestResults/. Please check the test output path."
                        }

                        // Беремо перший знайдений TRX файл
                        def trxFile = trxFiles[0].path
                        def junitFile = "${WORKSPACE}/TestResults/junit.xml"

                        // Створюємо папку TestResults, якщо вона ще не існує (на випадок, якщо її не створив --results-directory або coverlet)
                        sh "mkdir -p ${WORKSPACE}/TestResults"

                        echo "Converting TRX report to JUnit XML: ${trxFile} -> ${junitFile}"
                        // Викликаємо trx2junit через 'dotnet tool run'
                        sh "dotnet tool run trx2junit \"${trxFile}\" > \"${junitFile}\""

                        echo "Listing contents of TestResults directory after conversion:"
                        sh "ls -R ${WORKSPACE}/TestResults" // Перевірка, чи з'явився junit.xml

                        // Публікуємо JUnit звіт
                        junit "${junitFile}"
                    }
                }
            }
        }

        stage('Publish Coverage Report') {
            steps {
                // Тепер, коли JUnit звіт повинен бути коректним,
                // цей етап також повинен спрацювати без проблем.
                cobertura autoUpdateHealth: false,
                          autoUpdateStability: false,
                          coberturaReportFile: '**/TestResults/**/coverage.cobertura.xml',
                          failUnhealthy: false, // Set to true to fail build on unhealthy coverage
                          failUnstable: false,  // Set to true to fail build on unstable coverage
                          lineCoverageTargets: '0, 0, 0', // Example: '70, 80, 90' for thresholds (unhealthy, unstable, healthy)
                          onlyStable: false,
                          sourceEncoding: 'ASCII', // Or your project's encoding
                          stabilityTargets: '0, 0, 0' // Example: '70, 80, 90' for thresholds
            }
        }

        stage('Publish Application') {
            steps {
                echo "Publishing application (Solution: Assessment.sln)..."
                // Якщо ви публікуєте конкретний проект, вкажіть його:
                // sh 'dotnet publish SessionMVC/SessionMVC.csproj --no-build --configuration Release -o app/publish'
                sh 'dotnet publish Assessment.sln --no-build --configuration Release -o app/publish'
            }
        }

        stage('Build Docker Image') {
            steps {
                echo "Building Docker image (Image Name: sessionmvc)..."
                script {
                    def tags = env.IMAGE_TAGS.split(', ').collect { "-t ${it.trim()}" }.join(' ')
                    sh "docker build . ${tags} -f Dockerfile"
                }
            }
        }

        stage('Push Docker Image (Skipped)') {
            steps {
                echo "Skipping Docker image push for now."
            }
        }

        stage('Deploy to Environment') {
            steps {
                echo "Deploying to ${env.DEPLOY_ENV} environment..."
            }
        }

        stage('Git Tagging for Production') {
            steps {
                echo "Skipping Git tagging for Production for now."
            }
        }
    }

    post {
        always {
            cleanWs()
        }
        success {
            script {
                echo "Pipeline finished successfully for branch ${env.BRANCH_NAME} and environment ${env.DEPLOY_ENV}."
            }
        }
        failure {
            script {
                echo "Pipeline failed for branch ${env.BRANCH_NAME} and environment ${env.DEPLOY_ENV}!"
                // Не забувайте перевірити налаштування SMTP у Jenkins для сповіщень.
                // mail(to: 'your_email@example.com', subject: "Jenkins Build Failed: ${env.JOB_NAME} - ${env.BUILD_NUMBER}", body: "Build failed: ${env.BUILD_URL}")
            }
        }
    }
}
