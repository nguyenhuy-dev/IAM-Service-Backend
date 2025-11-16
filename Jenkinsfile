pipeline {
    // 1. Agent Configuration
    // Run on any available agent.
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
		// The credentialsId for jenkins. Change this if you want to use another account
		GITLAB_CREDENTIAL_ID = 'ae9ce4eb-57e8-45f5-af2d-200d0e7e843f'
        
        // The path to the folder containing your API's Dockerfile (at repo root)
        API_PROJECT_PATH      = 'IAMService.API' 
        APPLICATION_TEST_PROJECT_PATH = 'IAMService.Application.Test'
        
		// GCP Service Account Credentials (stored in Jenkins)
        // Create this credential in Jenkins: Manage Jenkins > Credentials > Add Credentials
        // Kind: "Secret file", upload your service account JSON key file
        GCP_CREDENTIALS_ID    = 'gcp-artifact-registry-sa'
		
        // --- GOOGLE CLOUD ARTIFACT REGISTRY ---
        GCP_REGISTRY          = 'asia-southeast1-docker.pkg.dev'
        GCP_PROJECT_ID        = 'lab-management-team01'
        GCP_REPOSITORY        = 'lab-management'
        DOCKER_IMAGE_NAME     = 'iam-service'
        
        // --- TEST PROJECT CONFIGURATION ---
        // Space-separated list of test project .csproj files to run
        // Example: 'IAMService.Application.Test/IAMService.Application.Test.csproj IAMService.API.Test/IAMService.API.Test.csproj'
        TEST_PROJECTS = 'IAMService.Application.Test/IAMService.Application.Test.csproj IAMService.API.Test/IAMService.API.Test.csproj'
        
        // --- CODE COVERAGE FILTER ---
        // Only collect coverage for these projects (exclude test projects and other assemblies)
        COVERAGE_INCLUDE = '[IAMService.Application]*,[IAMService.API]*'
        // Example: Exclude only from specific project
		COVERAGE_EXCLUDE = '[*.Test]*,[*]*.Program,[*]*Program*,[*]*.Startup,[*]*ErrorDetail,[*]*ErrorResponse,[*]*ValidationBehavior*,[*.Application]*.DTOs.*,[*.Application.DTOs]*,[*]*.gRPC.Protos.*,[*]*Grpc*'
        
        
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
                    echo "=== Checkout Information ==="
                    echo "GIT_BRANCH: ${env.GIT_BRANCH ?: 'N/A'}"
                    
                    // GitLab webhook specific (fallback)
                    echo "gitlabSourceBranch: ${env.gitlabSourceBranch ?: 'N/A'}"
                    echo "gitlabTargetBranch: ${env.gitlabTargetBranch ?: 'N/A'}"
                    echo "=============================="
                    
                    if (env.gitlabSourceBranch) {
                        // Fallback for GitLab webhook trigger
                        echo "🔀 This is a Merge Request build (GitLab Webhook)"
                        echo "📥 Checking out source branch: ${env.gitlabSourceBranch}"
                        
                        checkout([
                            $class: 'GitSCM',
                            branches: [[name: "origin/${env.gitlabSourceBranch}"]],
                            extensions: [
                                [$class: 'CleanBeforeCheckout'],
                                [$class: 'CloneOption', depth: 0, noTags: false, reference: '', shallow: false]
                            ],
                            userRemoteConfigs: [[
                                url: env.gitlabSourceRepoHttpUrl ?: env.GIT_URL,
                                credentialsId: env.GITLAB_CREDENTIAL_ID
                            ]]
                        ])
                        
                    } else {
                        // Regular branch build (not a merge request)
                        echo "🌿 This is a regular branch build"
                        checkout scm
                    }
                    
                    // Display current branch/commit info
                    sh '''
                        echo "---"
                        echo "Current commit:"
                        git log -1 --oneline
                        echo "---"
                        echo "Files in workspace:"
                        ls -la
                        echo "=============================="
                    '''
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
                    echo "Coverage will be collected ONLY for: ${env.COVERAGE_INCLUDE}"
                    
                    // 1. Define and write the .runsettings file
                    def runSettingsContent = """
                        <?xml version="1.0" encoding="utf-8"?>
                        <RunSettings>
                          <DataCollectionRunSettings>
                            <DataCollectors>
                              <DataCollector friendlyName="XPlat code coverage">
                                <Configuration>
                                  <DisableParallelProcessing>true</DisableParallelProcessing>
                                  <Include>${env.COVERAGE_INCLUDE}</Include>
                                  <Exclude>${env.COVERAGE_EXCLUDE}</Exclude>
                                </Configuration>
                              </DataCollector>
                            </DataCollectors>
                          </DataCollectionRunSettings>
                        </RunSettings>
                    """.stripIndent().trim()
                    
                    writeFile file: 'coverlet.runsettings', text: runSettingsContent
                    echo "Generated coverlet.runsettings file."

                    // 2. Split and loop through test projects
                    def testProjects = env.TEST_PROJECTS.split(' ')
                    
                    testProjects.each { testProjectPath ->
                        def projectName = testProjectPath.tokenize('/')[0]
                        
                        // THIS IS THE FIX: Create a unique temp dir for each project
                        def isolatedTempDir = "$WORKSPACE/coverlet-tmp/${projectName}"

                        echo "=========================================="
                        echo "Running tests for: ${testProjectPath}"
                        echo "Using settings: coverlet.runsettings"
                        echo "Using isolated temp dir: ${isolatedTempDir}"
                        echo "=========================================="
                        
                        sh """
                            # 3. Create the isolated dir and set TMPDIR
                            mkdir -p "${isolatedTempDir}"
                            export TMPDIR="${isolatedTempDir}"
                            
                            # 4. Run the test command
                            dotnet test "${testProjectPath}" \
                                --configuration ${BUILD_CONFIGURATION} \
                                --no-build \
                                --no-restore \
                                --logger "trx;LogFileName=${projectName}-results.trx" \
                                --logger "console;verbosity=detailed" \
                                --collect:"XPlat Code Coverage" \
                                --results-directory ./TestResults/${projectName} \
                                --settings "coverlet.runsettings"
                        """
                    }
                }
            }
            post {
                always {
                    script {
                        // Publish test results
                        if (fileExists('TestResults')) {
                            echo "Publishing test reports..."
                            // FIX: Using a recursive glob pattern to find all .trx files
                            junit testResults: 'TestResults/**/*.trx', 
                                    allowEmptyResults: true, 
                                    keepLongStdio: true
                        } else {
                            echo "Warning: 'TestResults' directory not found. Skipping test report publishing."
                        }
                    }
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
                        # FIX: Using a recursive glob (**) to find the files in their nested folders
                        ./tools/reportgenerator \
                            "-reports:TestResults/**/coverage.cobertura.xml" \
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

        stage('Docker Login to GCP') {
            steps {
                script {
                    echo "Authenticating Docker with GCP Artifact Registry..."
                    
                    // Use Jenkins credentials to authenticate
                    withCredentials([file(credentialsId: env.GCP_CREDENTIALS_ID, variable: 'GCP_KEY_FILE')]) {
                        sh """
                            # Activate service account using the key file
                            gcloud auth activate-service-account --key-file="\${GCP_KEY_FILE}"
                            
                            # Set the project
                            gcloud config set project ${GCP_PROJECT_ID}
                            
                            # Login to Docker using gcloud
                            gcloud auth print-access-token | docker login -u oauth2accesstoken --password-stdin ${GCP_REGISTRY}
                            
                            echo "✅ Successfully authenticated with GCP Artifact Registry"
                        """
                    }
                }
            }
        }
        
        stage('Build Docker Image') {
            steps {
                script {
                    echo "Building Docker image..."
                    def imageTag = "${BUILD_NUMBER}"
                    def fullImagePath = "${GCP_REGISTRY}/${GCP_PROJECT_ID}/${GCP_REPOSITORY}/${DOCKER_IMAGE_NAME}"
                    
                    sh """
                            # Build the Docker image
                            docker build -t ${DOCKER_IMAGE_NAME}:${imageTag} -f ${API_PROJECT_PATH}/Dockerfile .
                            
                            # Tag for GCP Artifact Registry with build number
                            docker tag ${DOCKER_IMAGE_NAME}:${imageTag} ${fullImagePath}:${imageTag}
                            
                            # Tag for GCP Artifact Registry with latest
                            docker tag ${DOCKER_IMAGE_NAME}:${imageTag} ${fullImagePath}:latest
                            
                            echo "Docker image built and tagged successfully:"
                            echo "  - ${fullImagePath}:${imageTag}"
                            echo "  - ${fullImagePath}:latest"
                            docker images | grep ${DOCKER_IMAGE_NAME}
                        """
                    
                    // Store image info for potential deployment
                    env.DOCKER_IMAGE_TAG = imageTag
                    env.FULL_IMAGE_PATH = fullImagePath
                    echo "Docker Image: ${fullImagePath}:${imageTag}"
                }
            }
        }
        
        stage('Push to GCP Artifact Registry') {
            steps {
                script {
                    echo "Pushing Docker images to GCP Artifact Registry..."
                    def fullImagePath = "${GCP_REGISTRY}/${GCP_PROJECT_ID}/${GCP_REPOSITORY}/${DOCKER_IMAGE_NAME}"
                    
                    sh """
                            # Push the tagged image
                            docker push ${fullImagePath}:${BUILD_NUMBER}
                            
                            # Push the latest tag
                            docker push ${fullImagePath}:latest
                            
                            echo "✅ Successfully pushed images to GCP Artifact Registry:"
                            echo "  - ${fullImagePath}:${BUILD_NUMBER}"
                            echo "  - ${fullImagePath}:latest"
                        """
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
               echo "Cleaning up build-specific Docker resources..."
               def fullImagePath = "${GCP_REGISTRY}/${GCP_PROJECT_ID}/${GCP_REPOSITORY}/${DOCKER_IMAGE_NAME}"
               // Remove local images to free up space
               sh """
                   docker rmi ${DOCKER_IMAGE_NAME}:${BUILD_NUMBER} || true
                   docker rmi ${fullImagePath}:${BUILD_NUMBER} || true
                   docker rmi ${fullImagePath}:latest || true
                   docker image prune -f || true
               """
            }
            cleanWs()
        }
        success {
            echo "✅ Pipeline completed successfully!"
        }
        failure {
            echo "❌ Pipeline failed. Check the logs for details."
        }
        unstable {
            echo "⚠️ Pipeline is unstable. Some tests may have failed."
        }
    }
}