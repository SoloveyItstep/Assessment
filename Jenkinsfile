pipeline {
  agent any

  environment {
    DOCKER_IMAGE = 'sessionmvc-app:latest'
    // потрібна змінна для mount в docker/compose:
    WORKSPACE = "${env.WORKSPACE}"
  }

  stages {

    stage('Checkout') {
      steps {
        checkout scm
      }
    }

    stage('Build & Test') {
      // збірка + тести у контейнері .NET
      agent {
        docker {
          image 'mcr.microsoft.com/dotnet/sdk:9.0'
          args  '-v /var/run/docker.sock:/var/run/docker.sock'
        }
      }
      steps {
        // Відновлення і збірка
        sh 'dotnet restore Assessment.sln'
        sh 'dotnet build Assessment.sln --configuration Release --no-restore'

        // Запуск тестів у TRX
        sh '''
          dotnet test Assessment.sln \
            --no-build --configuration Release \
            --logger "trx;LogFileName=testresults.trx" \
            --results-directory TestResults
        '''

        // Конвертація TRX → JUnit-XML
        sh 'mkdir -p TestResults && rm -f TestResults/*.xml'
        sh '''
          export PATH="$PATH:$HOME/.dotnet/tools"
          if ! command -v trx2junit >/dev/null 2>&1; then
            dotnet tool install --global trx2junit
          fi
          trx2junit TestResults/testresults.trx
        '''

        // Публікація в Jenkins
        junit 'TestResults/*.xml'
      }
    }

    stage('Docker Build') {
      steps {
        sh 'docker build -t $DOCKER_IMAGE .'
      }
    }

    stage('Start Dependencies') {
      steps {
        // Піднімаємо залежності з docker-compose.yml через офіційний образ
        sh '''
          docker run --rm \
            -v /var/run/docker.sock:/var/run/docker.sock \
            -v "$WORKSPACE":/workspace \
            -w /workspace \
            docker/compose:latest up -d
        '''
      }
    }

    stage('Run App Container') {
      steps {
        sh 'docker run -d -p 8081:5000 --name sessionmvc_container $DOCKER_IMAGE'
      }
    }
  }

  post {
    always {
      // Зупиняємо та видаляємо контейнер додатку
      sh 'docker stop sessionmvc_container || true'
      sh 'docker rm   sessionmvc_container   || true'

      // Прибираємо залежності через той самий образ compose
      sh '''
        docker run --rm \
          -v /var/run/docker.sock:/var/run/docker.sock \
          -v "$WORKSPACE":/workspace \
          -w /workspace \
          docker/compose:latest down || true
      '''
    }
  }
}
