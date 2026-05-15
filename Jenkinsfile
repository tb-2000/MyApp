pipeline {
    agent any
    
    environment {
        SUBSCRIPTION_ID = 'fe8af34a-02eb-4b77-9796-eee984bbad83'
        TENANT_ID = '0dba4b73-81df-44a7-9539-08ec0252d600'
        AZURE_CLI_PATH = 'C:\\Program Files\\Microsoft SDKs\\Azure\\CLI2\\wbin\\az.cmd'
    }
    
    stages {
        stage('Checkout') {
            steps {
                checkout scm
            }
        }
        
    stage('Setup Python & Dependencies') {
            steps {
                powershell '''
                    Write-Host "Python Version:"
                    python --version
                    
                    # Virtuelle Umgebung
                    python -m venv venv
                    .\\venv\\Scripts\\Activate.ps1
                    
                    Write-Host "Installing dependencies..."
                    pip install --upgrade pip
                    pip install -r requirements.txt
                '''
            }
        }
        
        stage('Test') {
            steps {
                powershell '''
                    .\\venv\\Scripts\\Activate.ps1
                    
                    pip install pytest pytest-cov
                    
                    Write-Host "Running tests..."
                    python -m pytest --cov=. --cov-report=xml --junitxml=test-results.xml
                '''
            }
        }
        
        stage('Package') {
            steps {
                powershell '''
                    Write-Host "Preparing deployment package..."
                    New-Item -ItemType Directory -Force -Path publish | Out-Null
                    
                    # Wichtige Dateien kopieren
                    Copy-Item -Path *.py, requirements.txt, runtime.txt, Procfile -Destination publish -Recurse -ErrorAction SilentlyContinue
                    Copy-Item -Path app, main.py, wsgi.py, asgi.py, templates, static -Destination publish -Recurse -ErrorAction SilentlyContinue
                    
                    Write-Host "Package ready."
                '''
                archiveArtifacts artifacts: 'publish/**', fingerprint: true
            }
        }
        
        stage('Deploy to Dev') {
            when { branch 'main' }
            steps {
                withCredentials([usernamePassword(credentialsId: 'azure-sp-credentials',
                                  usernameVariable: 'AZURE_CLIENT_ID',
                                  passwordVariable: 'AZURE_CLIENT_SECRET')]) {
                    
                    powershell '''
                        Write-Host "=== Azure Login ==="
                        & $env:AZURE_CLI_PATH login --service-principal `
                            -u $env:AZURE_CLIENT_ID `
                            -p $env:AZURE_CLIENT_SECRET `
                            --tenant $env:TENANT_ID
                        
                        & $env:AZURE_CLI_PATH account set --subscription $env:SUBSCRIPTION_ID
                        
                        Write-Host "=== Deploying to Dev ==="
                        cd publish
                        Compress-Archive -Path * -DestinationPath app.zip -Force
                        
                        & $env:AZURE_CLI_PATH webapp deployment source config-zip `
                            --resource-group "cicd-webapp" `
                            --name "cicd-webapp-dev" `
                            --src app.zip
                    '''
                }
            }
        }
    }
    
    post {
        always {
            deleteDir()
        }
    }
}