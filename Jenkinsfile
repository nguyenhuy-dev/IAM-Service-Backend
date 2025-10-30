pipeline {
    // 1. Agent Configuration
    // Run on any available agent.
    // This agent MUST have Docker installed and available on the PATH.
    agent any
    
    // 2. Tool Configuration
    // This requires you to configure a .NET SDK in Jenkins
    // under "Manage Jenkins" -> "Global Tool Configuration".
    // We've named it 'dotnet-sdk-9.0' here as an example.
    tools {
        dotnetsdk 'dotnet-sdk-9.0' 
    }
    
    // 3. Environment Variables
    // Configure these variables to match your project.
    environment {
        // --- PLEASE CONFIGURE THESE VALUES ---
        // The path to your solution file (at repo root)
        SOLUTION_FILE_PATH    = 'IAM-Service-Backend.sln'
        
        // The path to the folder containing your API's Dockerfile (at repo root)
        API_PROJECT_PATH      = 'IAMService.API' 
        APPLICATION_TEST_PROJECT_PATH = 'IAMService.Application.Test'
        
        // --- TEST PROJECT CONFIGURATION ---
        // Space-separated list of test project .csproj files to run
        // Example: 'IAMService.Application.Test/IAMService.Application.Test.csproj IAMService.API.Test/IAMService.API.Test.csproj'
        TEST_PROJECTS = 'IAMService.Application.Test/IAMService.Application.Test.csproj IAMService.API.Test/IAMService.API.Test.csproj'
        
        // --- CODE COVERAGE FILTER ---
        // Only collect coverage for these projects (exclude test projects and other assemblies)
        COVERAGE_INCLUDE = '[IAMService.Application]*,[IAMService.API]*'
        COVERAGE_EXCLUDE = '[*.Test]*,[*]*.Program,[*]*.Startup'
        
        // The full name for your Docker image (e.g., dockerhub-username/repo-name)
        DOCKER_IMAGE_NAME     = 'iamservice'
        
        // --- BUILD CONFIGURATION ---
        BUILD_CONFIGURATION   = 'Release'
        
        // --- DOTNET ENVIRONMENT VARIABLES ---
        DOTNET_CLI_HOME       = '/tmp/dotnet'
        DOTNET_SKIP_FIRST_TIME_EXPERIENCE = 'true'
        DOTNET_NOLOGO         = 'true'
    }
    
    stages {
        stage('Checkout') {
            steps {
                script {
                    echo "Checking out code from repository..."
                    checkout scm
                }
            }
        }
        
        stage('Setup Environment') {
            steps {
                script {
                    echo "Running on Linux/Unix agent"
                        sh '''
                            echo "Verifying .NET SDK installation..."
                            dotnet --version
                            dotnet --list-sdks
                            echo "Verifying Docker installation..."
                            docker --version
                        '''
                }
            }
        }
        
        stage('Restore Dependencies') {
            steps {
                script {
                    echo "Restoring NuGet packages..."
                    sh "dotnet restore ${SOLUTION_FILE_PATH}"
                }
            }
        }
        
        stage('Build') {
            steps {
                script {
                    echo "Building the solution..."
                    sh """
                            dotnet build ${SOLUTION_FILE_PATH} \
                                --configuration ${BUILD_CONFIGURATION} \
                                --no-restore
                        """
                }
            }
        }
        
        stage('Run Unit Tests') {
            steps {
                script {
                    echo "Running tests for specific test projects..."
                    echo "Coverage will be collected ONLY for: Application and API projects"
                    
                    // Split the TEST_PROJECTS string and run tests for each project
                    def testProjects = env.TEST_PROJECTS.split(' ')
                    
                    testProjects.each { testProjectPath ->
                        def projectName = testProjectPath.tokenize('/')[0]
                        echo "=========================================="
                        echo "Running tests for: ${testProjectPath}"
                        echo "Coverage filter: ${COVERAGE_INCLUDE}"
                        echo "=========================================="
                        
                        sh """
                            dotnet test "${testProjectPath}" \
                                --configuration ${BUILD_CONFIGURATION} \
                                --no-build \
                                --no-restore \
                                --logger "trx;LogFileName=${projectName}-results.trx" \
                                --logger "console;verbosity=detailed" \
                                --collect:"XPlat Code Coverage" \
                                --results-directory ./TestResults/${projectName} \
                                -- DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Include="${COVERAGE_INCLUDE}" \
                                   DataCollectionRunSettings.DataCollectors.DataCollector.Configuration.Exclude="${COVERAGE_EXCLUDE}"
                        """
                    }
                }
            }
            post {
                always {
                    script {
                        // Publish test results using JUnit format (MSTest plugin converts TRX to JUnit)
                        if (fileExists('TestResults')) {
                            junit testResults: '**/TestResults/**/*.trx', 
                                  allowEmptyResults: true,
                                  keepLongStdio: true
                        }
                    }
                }
            }
        }
        
        stage('Code Analysis') {
            steps {
                script {
                    echo "Running code analysis..."
                    sh """
                            dotnet format ${SOLUTION_FILE_PATH} --verify-no-changes --verbosity diagnostic || true
                        """
                }
            }
        }

        stage('Generate Coverage Report') {
            steps {
                script {
                    echo "📊 Generating code coverage report..."
                    sh '''
                        # Install ReportGenerator locally
                        dotnet tool install --tool-path ./tools dotnet-reportgenerator-globaltool || true
                        
                        # Find all coverage files
                        echo "Looking for coverage files..."
                        find ./TestResults -name "coverage.cobertura.xml" -type f
                        
                        # Generate HTML and Cobertura reports
                        ./tools/reportgenerator \
                            "-reports:**/TestResults/**/coverage.cobertura.xml" \
                            "-targetdir:./CoverageReport" \
                            "-reporttypes:Html;Cobertura;Badges;TextSummary" \
                            "-verbosity:Info" || true
                        
                        # Display summary
                        if [ -f ./CoverageReport/Summary.txt ]; then
                            echo "Coverage Summary:"
                            cat ./CoverageReport/Summary.txt
                        fi
                        
                        # List generated files
                        echo "Generated coverage files:"
                        ls -la ./CoverageReport/ || true
                    '''
                }
            }
            post {
                always {
                    script {
                        // Publish HTML Report
                        publishHTML([
                            reportDir: 'CoverageReport',
                            reportFiles: 'index.html',
                            reportName: 'Code Coverage Report',
                            allowMissing: true,
                            keepAll: true
                        ])
                        
                        // CRITICAL: Publish coverage to Jenkins
                        if (fileExists('CoverageReport/Cobertura.xml')) {
                            recordCoverage(
                                tools: [[parser: 'COBERTURA', pattern: 'CoverageReport/Cobertura.xml']],
                                sourceCodeRetention: 'EVERY_BUILD'
                            )
                        } else {
                            echo "⚠️ Warning: Cobertura.xml not found. Coverage will not be reported."
                        }
                    }
                }
            }
        }
        
        stage('Publish') {
            steps {
                script {
                    echo "Publishing the API project..."
                    sh """
                            dotnet publish ${API_PROJECT_PATH} \
                                --configuration ${BUILD_CONFIGURATION} \
                                --no-restore \
                                --no-build \
                                --output ./publish
                        """
                }
            }
        }
        
        stage('Build Docker Image') {
            steps {
                script {
                    echo "Building Docker image..."
                    def imageTag = "${BUILD_NUMBER}"
                    
                    sh """
                            docker build -t ${DOCKER_IMAGE_NAME}:${imageTag} -f ${API_PROJECT_PATH}/Dockerfile .
                            docker tag ${DOCKER_IMAGE_NAME}:${imageTag} ${DOCKER_IMAGE_NAME}:latest
                            echo "Docker image built successfully: ${DOCKER_IMAGE_NAME}:${imageTag}"
                            docker images | grep ${DOCKER_IMAGE_NAME}
                        """
                    
                    // Store image info for potential deployment
                    env.DOCKER_IMAGE_TAG = imageTag
                    echo "Docker Image: ${DOCKER_IMAGE_NAME}:${imageTag}"
                }
            }
        }
        
        stage('Security Scan') {
            steps {
                script {
                    echo "Running security scan on dependencies..."
                    sh """
                            dotnet list ${SOLUTION_FILE_PATH} package --vulnerable || true
                            dotnet list ${SOLUTION_FILE_PATH} package --outdated || true
                        """
                }
            }
        }
        
        stage('Archive Artifacts') {
            steps {
                script {
                    echo "Archiving build artifacts..."
                    archiveArtifacts artifacts: 'publish/**/*', 
                                     fingerprint: true, 
                                     allowEmptyArchive: true
                    archiveArtifacts artifacts: 'TestResults/**/*', 
                                     fingerprint: true, 
                                     allowEmptyArchive: true
                }
            }
        }
    }
    
    post {
        always {
            script {
                echo "Cleaning up Docker resources..."
                sh 'docker system prune -f || true'
            }
            cleanWs()
        }
        success {
            echo "✅ Pipeline completed successfully!"
            echo "Docker Image: ${DOCKER_IMAGE_NAME}:${env.DOCKER_IMAGE_TAG}"
        }
        failure {
            echo "❌ Pipeline failed. Check the logs for details."
        }
        unstable {
            echo "⚠️ Pipeline is unstable. Some tests may have failed."
        }
    }
}