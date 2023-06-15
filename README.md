# LinkedinEmails
![LinkedinEmails](https://user-images.githubusercontent.com/26238419/231917888-746674c1-f93e-4610-a1b5-6aec8a297ccf.png)
Searches for employees of a company on linkedin using [PuppeteerSharp](https://github.com/hardkoded/puppeteer-sharp) and outputs a list composed of full names and possible emails.<br />

It's now also possible to validate the emails for each employee using SMTP, note that this is not always 100% reliable.<br />

This application might stop working on the future due to changes in Linkedin, I will try to keep it up to date.<br />


## Building and Running

It's possible to create builds for Windows, Linux and macOS since the project is done in .NET Core 6.

### Step 1: Download and Install .NET Core 6.0 SDK

Download and install the .NET Core 6.0 SDK from the official website:
[.NET Core 6.0 SDK](https://dotnet.microsoft.com/download/dotnet/6.0)

Follow the installation instructions for your operating system.

### Step 2: Restore Dependencies
```bash
dotnet restore
```

### Step 3: Build the Project
```bash
dotnet build
```

### Step 4: Run the Project
```bash
dotnet run --project LinkedinSearcher/LinkedinSearcher.csproj
```

Make sure to pass the arguments needed.

### Step 5: Publish the Project (Optional)
```bash
dotnet publish --configuration Release --runtime linux-x64
```

If you want to build for other platform, check the official list of available options from microsoft:
[.NET RID Catalog](https://learn.microsoft.com/en-us/dotnet/core/rid-catalog)

## Usage
```bash
Usage: LinkedinEmails [-options]

options:
	-e <email>		your linkedin account email
	-p <password>		your linkedin account password
	-c <company name>	linkedin company email 
	-d <company domain>	email domain	
	-v			tries to validate the emails
	-f <file path>		generated emails filepath
	-pin <auth pin>     	authentication pin
	-h			show this help message and exit	
```

### Example
```bash
LinkedinEmails -e example@outlook.com -p 12345 -c twitter -d twitter.com
```

If you have 2FA activated on your account, you can specify the -pin argument and provide a authentication code
```bash
LinkedinEmails -e example@outlook.com -p 12345 -c twitter -d twitter.com -pin 274123
```

Validating emails:
```bash
LinkedinEmails -v -f generated-emails-2023-28-4-07-33-02.json
```