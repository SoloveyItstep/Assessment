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
    agent any 

    environment {
        APP_IMAGE_NAME = 'sessionmvc'
        DOTNET_SDK_VERSION = '9.0'
        ERROR_NOTIFICATION_EMAIL = 'your-email@example.com' // ЗАМІНІТЬ

        GIT_BRANCH_NAME              = "${env.BRANCH_NAME}"
        DEPLOY_ENVIRONMENT           = determineDeployEnvironment(env.BRANCH_NAME)
        ASPNETCORE_ENVIRONMENT_FOR_APP = determineAspNetCoreEnvironment(env.BRANCH_NAME)
        
        IMAGE_TAG_LATEST           = ""
        IMAGE_TAG_COMMIT           = ""
        IMAGE_TAG_ENV_SPECIFIC     = ""
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
                    
                    echo "Image tags will be: ${env.IMAGE_TAG_LATEST}, ${env.IMAGE_TAG_COMMIT}, ${env.IMAGE_TAG_ENV_SPECIFIC}"
                }
            }
        }

        stage('Build Application (.NET)') {
            agent {
                docker {
                    image "mcr.microsoft.com/dotnet/sdk:${env.DOTNET_SDK_VERSION}"
                }
            }
            steps {
                echo "Building the ASP.NET Core application (Solution: Assessment.sln)..."
                sh 'dotnet build Assessment.sln --configuration Release'
            }
        }

        stage('Test Application (.NET)') {
            agent {
                docker {
                    image "mcr.microsoft.com/dotnet/sdk:${env.DOTNET_SDK_VERSION}"
                }
            }
            steps {
                echo "Running .NET tests (Solution: Assessment.sln)..."
                sh 'dotnet test Assessment.sln --configuration Release --no-build'
            }
        }

        stage('Build Docker Image') {
            steps {
                script {
                    echo "Building Docker image with tags: ${env.IMAGE_TAG_LATEST}, ${env.IMAGE_TAG_COMMIT}, ${env.IMAGE_TAG_ENV_SPECIFIC}"
                    sh "docker build -t \"${env.IMAGE_TAG_LATEST}\" -t \"${env.IMAGE_TAG_COMMIT}\" -t \"${env.IMAGE_TAG_ENV_SPECIFIC}\" ."
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
                    
                    def composeFiles = "-f docker-compose.development.yml"
                    def overrideFileName = (env.DEPLOY_ENVIRONMENT != null && env.DEPLOY_ENVIRONMENT != "null") ? "docker-compose.${env.DEPLOY_ENVIRONMENT.toLowerCase()}.yml" : null
                    
                    if (overrideFileName != null && fileExists(overrideFileName)) {
                        composeFiles += " -f ${overrideFileName}"
                        echo "Using override file: ${overrideFileName}"
                    } else {
                        if (env.DEPLOY_ENVIRONMENT == 'Production' && overrideFileName != null) {
                            echo "WARNING: Production override file (${overrideFileName}) not found! Using default docker-compose.yml for Production."
                        } else if (overrideFileName != null || (env.DEPLOY_ENVIRONMENT != null && env.DEPLOY_ENVIRONMENT != "null")) {
                            echo "No specific override file found for ${env.DEPLOY_ENVIRONMENT} (${overrideFileName ?: 'N/A'}), using default docker-compose.yml."
                        } else {
                            echo "DEPLOY_ENVIRONMENT is null or invalid, using default docker-compose.development.yml."
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
            script { // <--- ДОДАНО SCRIPT БЛОК
                def finalBranchName = env.GIT_BRANCH_NAME ?: "unknown_branch (was null)"
                def finalDeployEnv = env.DEPLOY_ENVIRONMENT ?: "unknown_environment (was null)"
                echo "Pipeline finished for branch ${finalBranchName} and environment ${finalDeployEnv}."
            }
            cleanWs() // cleanWs() є кроком і може бути тут
        }
        success {
            // Тут можна додати кроки, якщо потрібно, наприклад, простий echo або script блок
            echo 'Pipeline succeeded!'
        }
        failure {
            script { // <--- ДОДАНО SCRIPT БЛОК
                def finalBranchName = env.GIT_BRANCH_NAME ?: "unknown_branch (was null)"
                def finalDeployEnv = env.DEPLOY_ENVIRONMENT ?: "unknown_environment (was null)"
                echo 'Pipeline failed!'
                if (env.ERROR_NOTIFICATION_EMAIL && env.ERROR_NOTIFICATION_EMAIL != 'your-email@example.com') {
                    mail to: "${env.ERROR_NOTIFICATION_EMAIL}",
                         subject: "FAILURE: Pipeline ${env.JOB_NAME} - Build #${env.BUILD_NUMBER} [${finalDeployEnv}]",
                         body: """Pipeline ${env.JOB_NAME} - Build #${env.BUILD_NUMBER} for environment ${finalDeployEnv} on branch ${finalBranchName} failed.
Check console output for more details: ${env.BUILD_URL}console"""
                } else {
                    echo "Email notification skipped: ERROR_NOTIFICATION_EMAIL not configured properly."
                }
            }
        }
    }
}
