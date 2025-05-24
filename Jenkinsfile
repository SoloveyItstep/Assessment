pipeline {
  agent {
    docker {
      image 'mcr.microsoft.com/dotnet/sdk:9.0'
      args '-v /var/run/docker.sock:/var/run/docker.sock'
    }
  }

  environment {
    DOCKER_IMAGE = 'sessionmvc-app:latest'
  }

  stages {
    stage('Checkout') {
      steps {
        git url: 'https://github.com/SoloveyItstep/Assessment.git', branch: 'master'
      }
    }

    stage('Restore Dependencies') {
      steps {
        sh 'dotnet restore Assessment.sln'
      }
    }

    stage('Build') {
      steps {
        sh 'dotnet build Assessment.sln --configuration Release --no-restore'
      }
    }

    stage('Test') {
      steps {
        sh 'dotnet test Assessment.sln --no-build --configuration Release --logger trx;LogFileName=testresults.trx --results-directory ./TestResults'
        junit 'TestResults/testresults.trx'
      }
    }

    stage('Build Docker Image') {
      steps {
        sh 'docker build -t $DOCKER_IMAGE .'
      }
    }

    stage('Start Dependencies') {
      steps {
        sh 'docker-compose up -d'
      }
    }

    stage('Run Docker Container') {
      steps {
        sh 'docker run -d -p 8081:5000 --name sessionmvc_container $DOCKER_IMAGE'
      }
    }
  }

  post {
    always {
      sh 'docker stop sessionmvc_container || true'
      sh 'docker rm sessionmvc_container || true'
    }
  }
}
