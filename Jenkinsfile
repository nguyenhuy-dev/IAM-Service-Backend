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
		// --- GITLAB CREDENTIAL ID ---
		GITLAB_CREDENTIAL_ID = '5b91663a-07b4-4fc3-b3b2-102d4303fcb1'
        
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
                    echo "CHANGE_ID: ${env.CHANGE_ID ?: 'N/A'}"
                    echo "CHANGE_TARGET: ${env.CHANGE_TARGET ?: 'N/A'}"
                    echo "CHANGE_BRANCH: ${env.CHANGE_BRANCH ?: 'N/A'}"
                    echo "BRANCH_NAME: ${env.BRANCH_NAME ?: 'N/A'}"
                    echo "GIT_BRANCH: ${env.GIT_BRANCH ?: 'N/A'}"
                    
                    // GitLab webhook specific (fallback)
                    echo "gitlabSourceBranch: ${env.gitlabSourceBranch ?: 'N/A'}"
                    echo "gitlabTargetBranch: ${env.gitlabTargetBranch ?: 'N/A'}"
                    echo "=============================="
                    
                    // GitLab Integration uses CHANGE_ID for merge requests
                    // CHANGE_BRANCH contains the source branch name
                    // CHANGE_TARGET contains the target branch (dev)
                    
                    if (env.CHANGE_ID) {
                        // This is a Merge Request (GitLab Integration)
                        echo "🔀 This is a Merge Request build (GitLab Integration)"
                        echo "📥 MR #${env.CHANGE_ID}: ${env.CHANGE_BRANCH} → ${env.CHANGE_TARGET}"
                        echo "✅ checkout scm will automatically use the source branch"
                        
                        // With GitLab Integration, 'checkout scm' automatically 
                        // checks out the correct source branch for merge requests
                        checkout scm
                        
                    } else if (env.gitlabSourceBranch) {
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
                        echo "=== Current Git Information ==="
                        echo "Current branch:"
                        git branch | grep \\* | cut -d ' ' -f2
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