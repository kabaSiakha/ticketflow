// Pipeline TicketFlow : vérifier la configuration Docker, puis construire
// les images du backend et du frontend à chaque push.
pipeline {
  agent any

  options {
    buildDiscarder(logRotator(numToKeepStr: '10'))
    disableConcurrentBuilds()
    timeout(time: 30, unit: 'MINUTES')
  }

  stages {
    stage('Informations') {
      steps {
        script {
          // L'étiquette des images = identifiant court du commit construit.
          env.TAG = sh(script: 'git rev-parse --short HEAD', returnStdout: true).trim()
        }
        echo "Branche : ${env.BRANCH_NAME ?: 'inconnue'} — commit : ${env.TAG}"
        sh 'docker version'
      }
    }

    stage('Vérifier docker-compose.yml') {
      steps {
        // Échoue si le fichier Compose contient une erreur de syntaxe.
        sh 'docker compose config --quiet'
      }
    }

    stage('Build backend') {
      steps {
        sh 'docker build -t ticketflow-backend:$TAG backend'
      }
    }

    stage('Build frontend') {
      steps {
        sh 'docker build -t ticketflow-frontend:$TAG frontend'
      }
    }
  }

  post {
    success {
      echo "Images construites : ticketflow-backend:${env.TAG} et ticketflow-frontend:${env.TAG}"
    }
    failure {
      echo "Échec : ouvre l'étape en rouge et lis la fin de sa sortie."
    }
  }
}
