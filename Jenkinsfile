pipeline {
  agent any

  environment {
    DOCKER_IMAGE = 'sessionmvc-app:latest'
  }

  stages {
    stage('Checkout') {
      steps {
        checkout scm
      }
    }

    stage('Build & Test') {
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
        sh 'rm -rf TestResults/*.xml'
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
        sh 'docker build -t $DOCKER_IMAGE .'
      }
    }

    stage('Start Dependencies') {
      steps {
        script {
          docker.image('docker/compose:1.29.2').inside(
            '-v /var/run/docker.sock:/var/run/docker.sock ' +
            "-v ${env.WORKSPACE}:${env.WORKSPACE} -w ${env.WORKSPACE}"
          ) {
            sh 'docker-compose up -d'
          }
        }
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
      // зупиняємо основний контейнер
      sh 'docker stop sessionmvc_container || true'
      sh 'docker rm   sessionmvc_container || true'
      // згортаємо залежності
      script {
        docker.image('docker/compose:1.29.2').inside(
          '-v /var/run/docker.sock:/var/run/docker.sock ' +
          "-v ${env.WORKSPACE}:${env.WORKSPACE} -w ${env.WORKSPACE}"
        ) {
          sh 'docker-compose down || true'
        }
      }
    }
  }
}
