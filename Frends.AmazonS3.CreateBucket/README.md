# Frends.AmazonS3.CreateBucket
Frends Task for creating a new bucket to AWS S3.

[![License: MIT](https://img.shields.io/badge/License-MIT-green.svg)](https://opensource.org/licenses/MIT) 
[![Build](https://github.com/FrendsPlatform/Frends.AmazonS3/actions/workflows/CreateBucket_build_and_test_on_main.yml/badge.svg)](https://github.com/FrendsPlatform/Frends.AmazonS3/actions)
![Coverage](https://app-github-custom-badges.azurewebsites.net/Badge?key=FrendsPlatform/Frends.AmazonS3/Frends.AmazonS3.CreateBucket|main)

# Installing

You can install the Task via frends UI Task View.

## Building

Rebuild the project

`dotnet build`

Run tests

"s3:CreateBucket" and "s3:DeleteBucket" action permissions required.
`dotnet test`

Create a NuGet package

`dotnet pack --configuration Release`