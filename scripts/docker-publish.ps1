param(
    [Parameter(Mandatory = $true)]
    [string] $DockerHubUser,

    [string] $ImageName = "mln111-api",
    [string] $Tag = "latest"
)

$ErrorActionPreference = "Stop"
$fullImage = "${DockerHubUser}/${ImageName}:${Tag}"

Write-Host "Build: $fullImage"
docker build -t $fullImage -f Dockerfile .

Write-Host "Push: $fullImage"
docker push $fullImage

Write-Host "Done. FE co the dung: $fullImage"
Write-Host "API: http://localhost:8080  Swagger: http://localhost:8080/swagger"
