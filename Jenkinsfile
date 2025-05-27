// Визначаємо допоміжні функції Groovy тут, ПОЗА блоком pipeline {}
def determineDeployEnvironment(String branchName) {
    if (branchName == null || branchName.isEmpty() || branchName == "null") {
        echo "WARNING: Branch name is null or empty in determineDeployEnvironment. Defaulting to FeatureBranch."
        return 'FeatureBranch'
    }
    if (branchName == 'master' || branchName == 'main') {
        return 'Production'
    } else if (branchName == 'develop') {
        return 'Development'
    } else {
        return 'FeatureBranch'
    }
}

def determineAspNetCoreEnvironment(String branchName) {
    if (branchName == null || branchName.isEmpty() || branchName == "null") {
        echo "WARNING: Branch name is null or empty in determineAspNetCoreEnvironment. Defaulting to Development."
        return 'Development'
    }
    if (branchName == 'master' || branchName == 'main') {
        return 'Production'
    } else if (branchName == 'develop') {
        return 'Development'
    } else {
        return 'Development'
    }
}

pipeline {
    // Агент для всього пайплайну. Можна залишити 'any', оскільки специфічні стадії використовують Docker.
    agent any

    environment {
        APP_IMAGE_NAME = 'sessionmvc'
        DOTNET_SDK_VERSION = '9.0'
        ERROR_NOTIFICATION_EMAIL = 'solovey.itstep@gmial.com' // Ваша пошта

        GIT_BRANCH_NAME              = "${env.BRANCH_NAME}"
        DEPLOY_ENVIRONMENT           = determineDeployEnvironment(env.BRANCH_NAME)
        ASPNETCORE_ENVIRONMENT_FOR_APP = determineAspNetCoreEnvironment(env.BRANCH_NAME)

        IMAGE_TAG_LATEST = null
        IMAGE_TAG_COMMIT = null
        IMAGE_TAG_ENV_SPECIFIC = null
    }

    stages {
        stage('Initialize and Display Environment') {
            steps {
                script {
                    echo "Current Git branch (from env.BRANCH_NAME via env.GIT_BRANCH_NAME): ${env.GIT_BRANCH_NAME}"
                    if (env.GIT_BRANCH_NAME == null || env.GIT_BRANCH_NAME.isEmpty() || env.GIT_BRANCH_NAME == "null") {
                        error "FATAL: Could not determine current Git branch. env.BRANCH_NAME was '${env.BRANCH_NAME}'"
                    }

                    echo "Deployment Environment: ${env.DEPLOY_ENVIRONMENT}"
                    echo "ASPNETCORE_ENVIRONMENT for application: ${env.ASPNETCORE_ENVIRONMENT_FOR_APP}"

                    def shortCommit = sh(script: 'git rev-parse --short HEAD', returnStdout: true).trim()
                    env.IMAGE_TAG_LATEST = "${env.APP_IMAGE_NAME}:latest"
                    env.IMAGE_TAG_COMMIT = "${env.APP_IMAGE_NAME}:${shortCommit}"

                    if (env.DEPLOY_ENVIRONMENT != null && env.DEPLOY_ENVIRONMENT != "null") {
                        env.IMAGE_TAG_ENV_SPECIFIC = "${env.APP_IMAGE_NAME}:${env.DEPLOY_ENVIRONMENT.toLowerCase()}-${shortCommit}"
                    } else {
                        env.IMAGE_TAG_ENV_SPECIFIC = "${env.APP_IMAGE_NAME}:unknownenv-${shortCommit}"
                        echo "WARNING: DEPLOY_ENVIRONMENT was null or 'null' when creating IMAGE_TAG_ENV_SPECIFIC."
                    }

                    echo "Image tags set to: ${env.IMAGE_TAG_LATEST}, ${env.IMAGE_TAG_COMMIT}, ${env.IMAGE_TAG_ENV_SPECIFIC}"
                }
            }
        }

        stage('Checkout') {
            steps {
                git url: 'https://github.com/SoloveyItstep/Assessment.git',
                    branch: 'master'
            }
        }

        stage('Restore Dependencies') {
            agent {
                docker {
                    image "mcr.microsoft.com/dotnet/sdk:${env.DOTNET_SDK_VERSION}"
                    // --- ДОДАНО: Монтування NuGet кешу ---
                    args '-v $HOME/.nuget:/root/.nuget'
                    // ---------------------------------
                }
            }
            steps {
                echo 'Restoring NuGet packages...'
                sh 'dotnet restore Assessment.sln'
            }
        }

        stage('Build Application (.NET)') {
            agent {
                docker {
                    image "mcr.microsoft.com/dotnet/sdk:${env.DOTNET_SDK_VERSION}"
                    // --- ДОДАНО: Монтування NuGet кешу ---
                    args '-v $HOME/.nuget:/root/.nuget'
                    // ---------------------------------
                }
            }
            steps {
                echo "Building the ASP.NET Core application (Solution: Assessment.sln)..."
                sh 'dotnet build Assessment.sln --configuration Release --no-restore'
            }
        }

        stage('Test and Collect Coverage') {
            agent {
                docker {
                    image "mcr.microsoft.com/dotnet/sdk:${env.DOTNET_SDK_VERSION}"
                    // --- ДОДАНО: Монтування NuGet кешу ---
                    args '-v $HOME/.nuget:/root/.nuget'
                     // ---------------------------------
                }
            }
            steps {
                echo "Running .NET tests and collecting coverage (Solution: Assessment.sln)..."
                // Генеруємо HTML звіт, а також Cobertura XML
                // Використовуємо ${WORKSPACE} для абсолютної вказівки шляху
                sh 'dotnet test Assessment.sln --configuration Release --no-build /p:CollectCoverage=true /p:CoverletOutputFormat="cobertura,html" /p:CoverletOutput=${WORKSPACE}/TestResults/ --results-directory ${WORKSPACE}/TestResults'
            }
        }

       stage('Test and Collect Coverage') {
            agent {
                docker {
                    image "mcr.microsoft.com/dotnet/sdk:${env.DOTNET_SDK_VERSION}"
                    args '-v $HOME/.nuget:/root/.nuget'
                }
            }
            steps {
                echo "Running .NET tests and collecting coverage (Solution: Assessment.sln)..."
                // Генеруємо **HTML** звіт, а також Cobertura XML (може знадобиться пізніше або для інших інструментів)
                // Coverlet Output Format 'html' генерує папку з HTML файлами
                // !!! ВИПРАВЛЕНО СИНТАКСИС /p:CoverletOutputFormat !!!
                sh 'dotnet test Assessment.sln --configuration Release --no-build /p:CollectCoverage=true /p:CoverletOutputFormat=cobertura,html /p:CoverletOutput=${WORKSPACE}/TestResults/ --results-directory ${WORKSPACE}/TestResults'
                // Шлях до HTML звіту буде приблизно: ${WORKSPACE}/TestResults/{GUID}/html/index.html
            }
        }

        stage('Build Docker Image') {
            steps {
                script {
                    echo "Building Docker image with tags: ${env.IMAGE_TAG_LATEST}, ${env.IMAGE_TAG_COMMIT}, ${env.IMAGE_TAG_ENV_SPECIFIC}"
                    def tagLatest = env.IMAGE_TAG_LATEST ?: "${env.APP_IMAGE_NAME}:latest-fallback"
                    def tagCommit = env.IMAGE_TAG_COMMIT ?: "${env.APP_IMAGE_NAME}:commit-fallback"
                    def tagEnvSpecific = env.IMAGE_TAG_ENV_SPECIFIC ?: "${env.APP_IMAGE_NAME}:env-fallback"
                    sh "docker build -t \"${tagLatest}\" -t \"${tagCommit}\" -t \"${tagEnvSpecific}\" ."
                }
            }
        }

        stage('Push Docker Image (Skipped)') {
            steps {
                echo "Skipping Docker Image Push as per configuration."
            }
        }

        stage('Deploy to Environment') {
            when {
                expression { env.DEPLOY_ENVIRONMENT == 'Development' || env.DEPLOY_ENVIRONMENT == 'Production' }
            }
            agent {
                docker {
                    image 'docker/compose:1.29.2'
                }
            }
            steps {
                script {
                    echo "Preparing to deploy to ${env.DEPLOY_ENVIRONMENT} environment using ASPNETCORE_ENVIRONMENT=${env.ASPNETCORE_ENVIRONMENT_FOR_APP}"

                    def composeFiles = "-f docker-compose.yml"
                    def overrideFileName = (env.DEPLOY_ENVIRONMENT != null && env.DEPLOY_ENVIRONMENT != "null") ? "docker-compose.${env.DEPLOY_ENVIRONMENT.toLowerCase()}.yml" : null

                    if (overrideFileName != null && fileExists(overrideFileName)) {
                        composeFiles += " -f ${overrideFileName}"
                        echo "Using override file: ${overrideFileName}"
                    } else {
                        if (env.DEPLOY_ENVIRONMENT == 'Production') {
                            echo "WARNING: Production override file ('${overrideFileName ?: 'docker-compose.production.yml'}') not found! Using only default docker-compose.yml for Production."
                        } else if (env.DEPLOYMENT_ENVIRONMENT == 'Development') { // Виправлено опечатку DEPLOYMENT_ENVIRONMENT
                             echo "INFO: Development override file ('${overrideFileName ?: 'docker-compose.development.yml'}') not found. Using only default docker-compose.yml for Development."
                        } else if (env.DEPLOY_ENVIRONMENT != "null" && env.DEPLOY_ENVIRONMENT != null) {
                            echo "INFO: No specific override file for ${env.DEPLOY_ENVIRONMENT} ('${overrideFileName}'). Using only default docker-compose.yml."
                        } else {
                            echo "WARNING: DEPLOY_ENVIRONMENT is null or invalid, using only default docker-compose.yml."
                        }
                    }

                    echo "Stopping and removing existing services (if any) using compose files: ${composeFiles}"
                    sh script: "docker-compose ${composeFiles} down --remove-orphans", returnStatus: true

                    echo "Deploying application using Docker Compose..."
                    sh "docker-compose --version"

                    echo "Executing: docker-compose ${composeFiles} up -d --build sessionmvc"
                    sh "docker-compose ${composeFiles} up -d --build sessionmvc"

                    echo "To check logs after deploy, run: docker-compose ${composeFiles} logs --tail=50 sessionmvc"
                }
            }
        }

        stage('Git Tagging for Production') {
            when {
                expression { env.DEPLOY_ENVIRONMENT == 'Production' }
            }
            steps {
                script {
                    def tagName = (env.DEPLOY_ENVIRONMENT != null && env.DEPLOY_ENVIRONMENT != "null") ? "v${new Date().format('yyyyMMdd.HHmmss')}-${env.DEPLOY_ENVIRONMENT.toLowerCase()}" : "v${new Date().format('yyyyMMdd.HHmmss')}-unknownenv"
                    echo "Creating Git tag: ${tagName}"
                    sh "git tag ${tagName}"
                    echo "Attempting to push Git tag: ${tagName}"
                    // sh "GIT_SSH_COMMAND='ssh -i ${GIT_SSH_KEY} -o IdentitiesOnly=yes -o StrictHostKeyChecking=no' git push origin ${tagName}"
                    echo "NOTE: 'git push origin ${tagName}' is currently commented out. Configure credentials and uncomment for actual push."
                }
            }
        }
    } // кінець stages

    post {
        always {
            script {
                def finalBranchName = env.GIT_BRANCH_NAME ?: "unknown_branch (was null)"
                def finalDeployEnv = env.DEPLOY_ENVIRONMENT ?: "unknown_environment (was null)"
                echo "Pipeline finished for branch ${finalBranchName} and environment ${finalDeployEnv}."
            }
            // cleanWs()
        }
        success {
            echo 'Pipeline succeeded!'
        }
        failure {
            script {
                def finalBranchName = env.GIT_BRANCH_NAME ?: "unknown_branch (was null)"
                def finalDeployEnv = env.DEPLOY_ENVIRONMENT ?: "unknown_environment (was null)"
                echo 'Pipeline failed!'
                // Виправлено опечатку в перевірці email адреси
                if (env.ERROR_NOTIFICATION_EMAIL && env.ERROR_NOTIFICATION_EMAIL != 'solovey.itstep@gmail.com') {
                     mail to: "${env.ERROR_NOTIFICATION_EMAIL}",
                         subject: "FAILURE: Pipeline ${env.JOB_NAME} - Build #${env.BUILD_NUMBER} [${finalDeployEnv}]",
                         body: """Pipeline ${env.JOB_NAME} - Build #${env.BUILD_NUMBER} for environment ${finalDeployEnv} on branch ${finalBranchName} failed.
Check console output for more details: ${env.BUILD_URL}console"""
                } else {
                    echo "Email notification skipped: ERROR_NOTIFICATION_EMAIL is default or not configured."
                }
            }
        }
    }
}
