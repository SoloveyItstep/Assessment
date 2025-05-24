pipeline {
    agent any 

    environment {
        APP_IMAGE_NAME = 'sessionmvc'
        DOTNET_SDK_VERSION = '9.0'
        ERROR_NOTIFICATION_EMAIL = 'your-email@example.com' // ЗАМІНІТЬ

        // Визначаємо змінні тут, використовуючи env.BRANCH_NAME, який має бути встановлений Jenkins
        // Ці блоки виконаються на початку пайплайну на агенті.
        GIT_BRANCH_NAME              = "${env.BRANCH_NAME}"
        DEPLOY_ENVIRONMENT           = determineDeployEnvironment(env.BRANCH_NAME)
        ASPNETCORE_ENVIRONMENT_FOR_APP = determineAspNetCoreEnvironment(env.BRANCH_NAME)
        
        // Теги образу також можна визначити тут, якщо shortCommit не потрібен динамічно на кожному етапі.
        // Але для shortCommit потрібен sh, тому залишимо їх визначення в script блоці.
        IMAGE_TAG_LATEST           = ""
        IMAGE_TAG_COMMIT           = ""
        IMAGE_TAG_ENV_SPECIFIC     = ""
    }

    // Допоміжні функції Groovy для визначення середовищ
    // Їх можна винести в Shared Library, якщо пайплайнів багато.
    def determineDeployEnvironment(String branchName) {
        if (branchName == 'master' || branchName == 'main') {
            return 'Production'
        } else if (branchName == 'develop') {
            return 'Development'
        } else {
            return 'FeatureBranch' // Або інше дефолтне значення для інших гілок
        }
    }

    def determineAspNetCoreEnvironment(String branchName) {
        if (branchName == 'master' || branchName == 'main') {
            return 'Production'
        } else if (branchName == 'develop') {
            return 'Development'
        } else {
            return 'Development' // Для feature-гілок зазвичай використовують Development налаштування
        }
    }

    stages {
        stage('Initialize and Display Environment') {
            steps {
                script {
                    // Перевіряємо, чи змінні середовища встановлені коректно
                    echo "Current Git branch (from env.BRANCH_NAME): ${env.GIT_BRANCH_NAME}"
                    if (env.GIT_BRANCH_NAME == null || env.GIT_BRANCH_NAME.isEmpty() || env.GIT_BRANCH_NAME == "null") {
                        error "FATAL: Could not determine current Git branch. env.BRANCH_NAME is '${env.GIT_BRANCH_NAME}'"
                    }
                    
                    echo "Deployment Environment: ${env.DEPLOY_ENVIRONMENT}"
                    echo "ASPNETCORE_ENVIRONMENT for application: ${env.ASPNETCORE_ENVIRONMENT_FOR_APP}"

                    // Визначаємо теги для Docker-образу (тут, бо потрібен checkout для shortCommit)
                    def shortCommit = sh(script: 'git rev-parse --short HEAD', returnStdout: true).trim()
                    env.IMAGE_TAG_LATEST = "${env.APP_IMAGE_NAME}:latest"
                    env.IMAGE_TAG_COMMIT = "${env.APP_IMAGE_NAME}:${shortCommit}"
                    // Перевірка на null перед toLowerCase()
                    if (env.DEPLOY_ENVIRONMENT != null) {
                        env.IMAGE_TAG_ENV_SPECIFIC = "${env.APP_IMAGE_NAME}:${env.DEPLOY_ENVIRONMENT.toLowerCase()}-${shortCommit}"
                    } else {
                        // Це не повинно трапитися, якщо GIT_BRANCH_NAME визначено
                        env.IMAGE_TAG_ENV_SPECIFIC = "${env.APP_IMAGE_NAME}:unknownenv-${shortCommit}" 
                        echo "WARNING: DEPLOY_ENVIRONMENT was null when creating IMAGE_TAG_ENV_SPECIFIC."
                    }
                    
                    echo "Image tags will be: ${env.IMAGE_TAG_LATEST}, ${env.IMAGE_TAG_COMMIT}, ${env.IMAGE_TAG_ENV_SPECIFIC}"
                }
            }
        }

        stage('Build Application (.NET)') {
            // ... (решта етапів залишаються такими ж, як у попередній версії)
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
                echo "Building Docker image with tags: ${env.IMAGE_TAG_LATEST}, ${env.IMAGE_TAG_COMMIT}, ${env.IMAGE_TAG_ENV_SPECIFIC}"
                sh "docker build -t ${env.IMAGE_TAG_LATEST} -t ${env.IMAGE_TAG_COMMIT} -t ${env.IMAGE_TAG_ENV_SPECIFIC} ."
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
                    // Перевірка на null перед toLowerCase()
                    def overrideFileName = env.DEPLOY_ENVIRONMENT != null ? "docker-compose.${env.DEPLOY_ENVIRONMENT.toLowerCase()}.yml" : null
                    
                    if (overrideFileName != null && fileExists(overrideFileName)) {
                        composeFiles += " -f ${overrideFileName}"
                        echo "Using override file: ${overrideFileName}"
                    } else {
                        if (env.DEPLOY_ENVIRONMENT == 'Production' && overrideFileName != null) {
                            echo "WARNING: Production override file (${overrideFileName}) not found! Using default docker-compose.yml for Production."
                        } else if (overrideFileName != null) {
                            echo "No specific override file found for ${env.DEPLOY_ENVIRONMENT} (${overrideFileName}), using default docker-compose.yml."
                        } else {
                            echo "DEPLOY_ENVIRONMENT is null, using default docker-compose.yml."
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
                    // ... (код для тегування, як у попередній версії)
                    def tagName = "v${new Date().format('yyyyMMdd.HHmmss')}-${env.DEPLOY_ENVIRONMENT.toLowerCase()}"
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
            def finalBranchName = env.GIT_BRANCH_NAME ?: "unknown_branch (was null)"
            def finalDeployEnv = env.DEPLOY_ENVIRONMENT ?: "unknown_environment (was null)"
            echo "Pipeline finished for branch ${finalBranchName} and environment ${finalDeployEnv}."
            cleanWs()
        }
        success {
            // ...
        }
        failure {
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
