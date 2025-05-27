pipeline {
    agent {
        docker {
            image 'mcr.microsoft.com/dotnet/sdk:9.0'
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
                // Встановлюємо trx2junit. Він буде доданий до ~/.dotnet/tools, але не обов'язково до PATH сесії.
                sh 'dotnet tool install -g trx2junit'
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
                // *** ЗМІНА ТУТ: Тепер запускаємо тести на конкретному тестовому проекті ***
                // Це забезпечить більш передбачуване розміщення результатів у --results-directory
                sh 'dotnet test Session.UnitTests/Session.UnitTests.csproj ' +
                   '--configuration Release ' +
                   '--no-build ' +
                   '/p:CollectCoverage=true ' +
                   '/p:CoverletOutputFormat=cobertura ' +
                   '/p:CoverletOutput="${WORKSPACE}/TestResults/coverage.xml" ' + // Цей шлях для CoverletOK
                   '--results-directory "${WORKSPACE}/TestResults"' // Результати VSTest (TRX) підуть сюди
            }
            post {
                always {
                    echo "Listing contents of TestResults directory for conversion (before conversion):"
                    // Цей 'ls -R' тепер повинен показати вміст, включаючи GUID-папку
                    sh "ls -R ${WORKSPACE}/TestResults"

                    script {
                        // Знаходимо TRX файл. Він буде у форматі .trx і, ймовірно, у GUID-папці.
                        // Використовуємо findFiles з Jenkins, це більш надійно, ніж sh find.
                        def testResultFiles = findFiles(glob: "${WORKSPACE}/TestResults/**/*.trx")

                        if (testResultFiles.length == 0) {
                            // Якщо .trx не знайдено, спробуємо знайти будь-який .xml, якщо формат змінився.
                            testResultFiles = findFiles(glob: "${WORKSPACE}/TestResults/**/*.xml")
                        }

                        if (testResultFiles.length == 0) {
                            error "TRX or XML test results file not found in ${WORKSPACE}/TestResults/. Cannot publish JUnit report."
                        }

                        // Беремо перший знайдений файл результатів
                        def trxFile = testResultFiles[0].path

                        def junitFile = "${WORKSPACE}/TestResults/junit.xml"

                        echo "Converting test report to JUnit XML: ${trxFile} -> ${junitFile}"
                        // *** ЗМІНА ТУТ: Викликаємо trx2junit через 'dotnet tool run' ***
                        // Це найбезпечніший спосіб викликати глобальний інструмент .NET у пайплайні,
                        // оскільки він не залежить від змінних середовища PATH.
                        sh "dotnet tool run trx2junit \"${trxFile}\" > \"${junitFile}\""

                        echo "Listing contents of TestResults directory after conversion:"
                        sh "ls -R ${WORKSPACE}/TestResults" // Перевіряємо, чи з'явився junit.xml

                        // Публікуємо JUnit звіт з коректним шляхом
                        junit "${junitFile}"
                    }
                }
            }
        }

        stage('Publish Coverage Report') {
            steps {
                // Цей шлях до coverage.xml повинен залишатися коректним,
                // оскільки CoverletOutput був налаштований на цю директорію.
                cobertura coberturaReportFile: '**/TestResults/coverage.xml',
                          lineCoverageTargets: '80, 90, 95',
                          branchCoverageTargets: '70, 80, 90',
                          failUnhealthy: true,
                          failUnstable: true
            }
        }

        stage('Publish Application') {
            steps {
                echo "Publishing application (Solution: Assessment.sln)..."
                // Якщо ви публікуєте конкретний проект, вкажіть його:
                // sh 'dotnet publish Assessment/SessionMVC/SessionMVC.csproj --no-build --configuration Release -o app/publish'
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
                // Не забувайте про налаштування SMTP у Jenkins для сповіщень.
                // mail(to: 'your_email@example.com', subject: "Jenkins Build Failed: ${env.JOB_NAME} - ${env.BUILD_NUMBER}", body: "Build failed: ${env.BUILD_URL}")
            }
        }
    }
}
