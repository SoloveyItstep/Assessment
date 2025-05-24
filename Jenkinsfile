pipeline {
  agent any

  environment {
    DOCKER_IMAGE = 'sessionmvc-app:latest'
  }

  stages {
    stage('Checkout') {
      steps {
        // Клонування коду
        checkout scm
      }
    }

    stage('Build & Test') {
      // Усередині офіційного .NET SDK-контейнера
      agent {
        docker {
          image 'mcr.microsoft.com/dotnet/sdk:9.0'
          args  '-v /var/run/docker.sock:/var/run/docker.sock'
        }
      }
      steps {
        sh 'dotnet restore Assessment.sln'
        sh 'dotnet build Assessment.sln --configuration Release --no-restore'
        sh '''
          dotnet test Assessment.sln \
            --no-build --configuration Release \
            --logger "trx;LogFileName=testresults.trx" \
            --results-directory TestResults
        '''
        sh 'mkdir -p TestResults'
        sh 'rm -f TestResults/testresults.xml'
        sh '''
          export PATH="$PATH:$HOME/.dotnet/tools"
          if ! command -v trx2junit >/dev/null 2>&1; then
            dotnet tool install --global trx2junit
          fi
          trx2junit TestResults/testresults.trx
        '''
        junit 'TestResults/*.xml'
      }
    }

    stage('Docker Build') {
      steps {
        // Збираємо ваш образ
        sh 'docker build -t $DOCKER_IMAGE .'
      }
    }

    stage('Start Dependencies') {
      steps {
        // Піднімаємо Mongo, Redis, RabbitMQ, ваш сервіс за допомогою docker-compose
        sh '''
          docker run --rm \
            -v /var/run/docker.sock:/var/run/docker.sock \
            -v $WORKSPACE:$WORKSPACE \
            -w $WORKSPACE \
            docker/compose:1.29.2 \
            up -d
        '''
      }
    }

    stage('Run App Container') {
      steps {
        // Запускаємо вашу апку на 8081
        sh 'docker run -d -p 8081:5000 --name sessionmvc_container $DOCKER_IMAGE'
      }
    }
  }

  post {
    always {
      // Завжди зупиняємо і видаляємо контейнер апки
      sh 'docker stop sessionmvc_container || true'
      sh 'docker rm sessionmvc_container   || true'
      // І згортаємо залежності
      sh '''
        docker run --rm \
          -v /var/run/docker.sock:/var/run/docker.sock \
          -v $WORKSPACE:$WORKSPACE \
          -w $WORKSPACE \
          docker/compose:1.29.2 \
          down
      '''
    }
  }
}
