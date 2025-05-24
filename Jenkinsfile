pipeline {
    agent any

    environment {
        APP_IMAGE_NAME = 'sessionmvc'
        DOTNET_SDK_VERSION = '9.0'
        ERROR_NOTIFICATION_EMAIL = 'solovey.itstep@gmail.com' // ЗАМІНІТЬ НА ВАШУ АДРЕСУ

        // Ці змінні будуть визначені на етапі Initialize Environment
        GIT_BRANCH_NAME = '' // Для зберігання імені поточної гілки
        DEPLOY_ENVIRONMENT = '' 
        ASPNETCORE_ENVIRONMENT_FOR_APP = '' // Це значення буде передано в контейнер
    }

    stages {
        stage('Initialize Environment and Checkout') {
            steps {
                // Спочатку робимо checkout, щоб визначити гілку
                checkout scm
                script {
                    env.GIT_BRANCH_NAME = scm.branches[0].name // Отримуємо ім'я поточної гілки
                    echo "Current Git branch: ${env.GIT_BRANCH_NAME}"

                    // Визначаємо середовище на основі гілки Git
                    if (env.GIT_BRANCH_NAME == 'master' || env.GIT_BRANCH_NAME == 'main') {
                        env.DEPLOY_ENVIRONMENT = 'Production'
                        env.ASPNETCORE_ENVIRONMENT_FOR_APP = 'Production'
                    } else if (env.GIT_BRANCH_NAME == 'develop') {
                        env.DEPLOY_ENVIRONMENT = 'Development'
                        env.ASPNETCORE_ENVIRONMENT_FOR_APP = 'Development'
                    } else {
                        // Для інших гілок можна встановити Development за замовчуванням
                        // або фейлити пайплайн, якщо вони не призначені для деплою
                        env.DEPLOY_ENVIRONMENT = 'Development' // Або 'FeatureBranch' тощо.
                        env.ASPNETCORE_ENVIRONMENT_FOR_APP = 'Development'
                        echo "Branch '${env.GIT_BRANCH_NAME}' is not 'master' or 'develop'. Defaulting to Development environment for ASP.NET Core."
                        // Якщо для feature-гілок не потрібен повний деплой, тут можна змінити логіку
                    }
                    echo "Deployment Environment: ${env.DEPLOY_ENVIRONMENT}"
                    echo "ASPNETCORE_ENVIRONMENT for application: ${env.ASPNETCORE_ENVIRONMENT_FOR_APP}"

                    // Визначаємо теги для Docker-образу
                    def shortCommit = sh(script: 'git rev-parse --short HEAD', returnStdout: true).trim()
                    env.IMAGE_TAG_LATEST = "${env.APP_IMAGE_NAME}:latest" // Завжди збираємо latest
                    env.IMAGE_TAG_COMMIT = "${env.APP_IMAGE_NAME}:${shortCommit}"
                    env.IMAGE_TAG_ENV_SPECIFIC = "${env.APP_IMAGE_NAME}:${env.DEPLOY_ENVIRONMENT.toLowerCase()}-${shortCommit}"
                    
                    echo "Image tags will be: ${env.IMAGE_TAG_LATEST}, ${env.IMAGE_TAG_COMMIT}, ${env.IMAGE_TAG_ENV_SPECIFIC}"
                }
            }
        }

        // Примітка: checkout на початку кожного docker agent блоку (як було раніше) не потрібен,
        // якщо код вже завантажено на першому етапі і робочий простір передається.
        // Jenkins монтує робочий простір в Docker-контейнери.

        stage('Build Application (.NET)') {
            agent {
                docker {
                    image "mcr.microsoft.com/dotnet/sdk:${env.DOTNET_SDK_VERSION}"
                }
            }
            steps {
                echo "Building the ASP.NET Core application (Solution: Assessment.sln)..."
                // ASPNETCORE_ENVIRONMENT_FOR_APP тут не використовується для `dotnet build`, 
                // оскільки `appsettings.*.json` файли копіюються в Dockerfile на етапі publish.
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
                // Під час `docker build` ASPNETCORE_ENVIRONMENT всередині Dockerfile (якщо він там встановлений)
                // визначить, який appsettings буде "активним" під час PUBLISH.
                // Ваш Dockerfile копіює всі файли, а потім ENV ASPNETCORE_HTTP_PORTS=5000.
                // ASPNETCORE_ENVIRONMENT для publish береться з середовища, де виконується `dotnet publish`.
                // Щоб це було більш керовано, можна передати ASPNETCORE_ENVIRONMENT_FOR_APP як build-arg:
                // sh "docker build --build-arg APP_ENV=${env.ASPNETCORE_ENVIRONMENT_FOR_APP} -t ${env.IMAGE_TAG_LATEST} -t ${env.IMAGE_TAG_COMMIT} -t ${env.IMAGE_TAG_ENV_SPECIFIC} ."
                // І в Dockerfile:
                // ARG APP_ENV=Production (дефолтне значення)
                // ENV ASPNETCORE_ENVIRONMENT=$APP_ENV
                // RUN dotnet publish ... (ASPNETCORE_ENVIRONMENT буде використано)
                // АЛЕ, оскільки ваш Dockerfile копіює всі appsettings.*.json, і ми будемо встановлювати 
                // ASPNETCORE_ENVIRONMENT під час `docker-compose up`, то передавати його як build-arg не є критично необхідним
                // для вибору правильного `appsettings` під час виконання. Це більше вплине, якщо `dotnet publish`
                // робить трансформації конфігів на основі ASPNETCORE_ENVIRONMENT.
                // Для простоти поки залишимо як є.
                sh "docker build -t ${env.IMAGE_TAG_LATEST} -t ${env.IMAGE_TAG_COMMIT} -t ${env.IMAGE_TAG_ENV_SPECIFIC} ."
            }
        }

        stage('Push Docker Image (Skipped)') {
            steps {
                echo "Skipping Docker Image Push as per configuration."
            }
        }

        stage('Deploy to Environment') {
            agent {
                docker {
                    image 'docker/compose:1.29.2'
                    // args '-v /var/run/docker.sock:/var/run/docker.sock' // Розкоментуйте, якщо потрібно
                }
            }
            steps {
                script {
                    echo "Preparing to deploy to ${env.DEPLOY_ENVIRONMENT} environment using ASPNETCORE_ENVIRONMENT=${env.ASPNETCORE_ENVIRONMENT_FOR_APP}"
                    
                    def composeFiles = "-f docker-compose.yml"
                    // Перевіряємо, чи існує spezifischer override-файл для поточного середовища
                    def overrideFileName = "docker-compose.${env.DEPLOY_ENVIRONMENT.toLowerCase()}.yml"
                    if (fileExists(overrideFileName)) {
                        composeFiles += " -f ${overrideFileName}"
                        echo "Using override file: ${overrideFileName}"
                    } else {
                        echo "No specific override file found for ${env.DEPLOY_ENVIRONMENT} (${overrideFileName}), using default docker-compose.yml."
                    }

                    echo "Stopping and removing existing services (if any)..."
                    sh script: "docker-compose ${composeFiles} down --remove-orphans", returnStatus: true
                    
                    echo "Deploying application using Docker Compose..."
                    sh "docker-compose --version"

                    // Встановлюємо ASPNETCORE_ENVIRONMENT для команди docker-compose up.
                    // Це значення буде передано в контейнер sessionmvc, ЯКЩО в docker-compose файлі (або override)
                    // для сервісу sessionmvc в секції environment є запис типу:
                    // - ASPNETCORE_ENVIRONMENT=${ASPNETCORE_ENVIRONMENT}
                    // АБО
                    // - ASPNETCORE_ENVIRONMENT 
                    // (останнє означає, що змінна з таким ім'ям береться з оточення, де запущено docker-compose)
                    // Оскільки ми хочемо, щоб саме env.ASPNETCORE_ENVIRONMENT_FOR_APP з Jenkins керував цим,
                    // потрібно, щоб docker-compose.yml (або відповідний override) був налаштований на це.
                    
                    // Припустимо, ваш docker-compose.yml (або docker-compose.development.yml / docker-compose.production.yml)
                    // має такий запис для сервісу sessionmvc:
                    // services:
                    //   sessionmvc:
                    //     environment:
                    //       - ASPNETCORE_ENVIRONMENT=${ASPNETCORE_ENVIRONMENT_FROM_HOST:-Development}
                    //       - ASPNETCORE_URLS=http://+:5000
                    //       - MongoConnectionString=${MONGO_CONNECTION_STRING_FROM_HOST}
                    //       - ConnectionStrings__AssessmentDbConnectionString=${SQL_CONNECTION_STRING_FROM_HOST}

                    // Тоді ми можемо передати ці змінні так:
                    def command = """
                        ASPNETCORE_ENVIRONMENT_FROM_HOST='${env.ASPNETCORE_ENVIRONMENT_FOR_APP}' \\
                        MONGO_CONNECTION_STRING_FROM_HOST='mongodb://root:example@mongo:27017/Assessment?authSource=admin&directConnection=true' \\
                        SQL_CONNECTION_STRING_FROM_HOST='Server=db;Database=Assessment;User=sa;Password=Your_password123;Encrypt=False;TrustServerCertificate=True' \\
                        docker-compose ${composeFiles} up -d --build sessionmvc
                    """
                    // Зверніть увагу на одинарні лапки навколо значень змінних, щоб уникнути проблем з спеціальними символами.
                    // І на `\` для перенесення рядків у Groovy multi-line string.
                    
                    // Однак, якщо у вашому docker-compose.yml для sessionmvc вже жорстко прописані ці змінні,
                    // наприклад, ASPNETCORE_ENVIRONMENT=Development, то для Production вам АБСОЛЮТНО ТОЧНО
                    // потрібен override-файл (наприклад, docker-compose.production.yml), де буде
                    // ASPNETCORE_ENVIRONMENT=Production. І тоді команда буде простішою:
                    // sh "docker-compose ${composeFiles} up -d --build sessionmvc"
                    // А env.ASPNETCORE_ENVIRONMENT_FOR_APP буде використовуватися тільки для логіки в Jenkins.
                    
                    // Давайте приймемо другий, простіший варіант:
                    // 1. Ваш `docker-compose.yml` містить конфігурацію для Development (ASPNETCORE_ENVIRONMENT=Development).
                    // 2. Ви створюєте `docker-compose.production.yml`, який перекриває ASPNETCORE_ENVIRONMENT на Production.
                    // JenkinsFile просто вибирає, які файли використовувати.
                    // `docker-compose.yml` (основний):
                    //   services:
                    //     sessionmvc:
                    //       environment:
                    //         - ASPNETCORE_ENVIRONMENT=Development
                    //         - ASPNETCORE_URLS=http://+:5000
                    //         - MongoConnectionString=mongodb://root:example@mongo:27017/Assessment?authSource=admin&directConnection=true
                    //         - ConnectionStrings__AssessmentDbConnectionString=Server=db;Database=Assessment;User=sa;Password=Your_password123;Encrypt=False;TrustServerCertificate=True
                    // `docker-compose.production.yml` (створіть цей файл):
                    //   version: '3.8' # Або ваша версія
                    //   services:
                    //     sessionmvc:
                    //       environment:
                    //         ASPNETCORE_ENVIRONMENT: Production # Перекриваємо для Production
                                         // Інші змінні, якщо вони відрізняються для Production, можна теж тут перекрити

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
                    // Потрібно налаштувати credentials для push в Git
                    // withCredentials([sshUserPrivateKey(credentialsId: 'your-git-ssh-credentials-id', keyFileVariable: 'GIT_SSH_KEY')]) {
                        // sh 'git config --global user.email "jenkins@example.com"'
                        // sh 'git config --global user.name "Jenkins CI"'
                        
                        // Використовуємо вже визначений env.IMAGE_TAG_COMMIT або env.GIT_BRANCH_NAME
                        def tagName = "v${new Date().format('yyyyMMdd.HHmmss')}-${env.DEPLOY_ENVIRONMENT.toLowerCase()}"
                        echo "Creating Git tag: ${tagName}"
                        sh "git tag ${tagName}"
                        echo "Attempting to push Git tag: ${tagName}"
                        // sh "git push origin ${tagName}" // Розкоментуйте, коли налаштуєте credentials для push
                        echo "NOTE: 'git push origin ${tagName}' is currently commented out. Configure credentials and uncomment for actual push."
                    // }
                }
            }
        }
    }

    post {
        always {
            echo "Pipeline finished for branch ${env.GIT_BRANCH_NAME} and environment ${env.DEPLOY_ENVIRONMENT}."
            // Очищення робочої області Jenkins
            cleanWs()
        }
        success {
            echo 'Pipeline succeeded!'
            // mail to: "${env.ERROR_NOTIFICATION_EMAIL}",
            //      subject: "SUCCESS: Pipeline ${env.JOB_NAME} - Build #${env.BUILD_NUMBER} [${env.DEPLOY_ENVIRONMENT}]",
            //      body: "Pipeline ${env.JOB_NAME} - Build #${env.BUILD_NUMBER} for ${env.DEPLOY_ENVIRONMENT} on branch ${env.GIT_BRANCH_NAME} completed successfully. URL: ${env.BUILD_URL}"
        }
        failure {
            echo 'Pipeline failed!'
            mail to: "${env.ERROR_NOTIFICATION_EMAIL}",
                 subject: "FAILURE: Pipeline ${env.JOB_NAME} - Build #${env.BUILD_NUMBER} [${env.DEPLOY_ENVIRONMENT}]",
                 body: """Pipeline ${env.JOB_NAME} - Build #${env.BUILD_NUMBER} for environment ${env.DEPLOY_ENVIRONMENT} on branch ${env.GIT_BRANCH_NAME} failed.
Check console output for more details: ${env.BUILD_URL}console"""
        }
    }
}
