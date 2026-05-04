pipeline {
    agent any
    
    environment {
        DOTNET_VERSION = '8.0'
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
        
        stage('Restore & Build') {
            steps {
                powershell 'dotnet restore'
                powershell 'dotnet build --configuration Release --no-restore'
            }
        }
        
        stage('Test') {
            steps {
                powershell 'dotnet test --configuration Release --no-build'
            }
        }
        
        stage('Publish') {
            steps {
                powershell 'dotnet publish src\\DeinProjekt.Web\\DeinProjekt.Web.csproj --configuration Release --output publish --no-build'
                archiveArtifacts artifacts: 'publish/**', fingerprint: true
            }
        }
        
        stage('Deploy to Dev') {
            when { branch 'develop' }
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
            cleanWs()
        }
    }
}