# LinkedinEmails
![linkedinemails](https://user-images.githubusercontent.com/26238419/231872110-21f60d32-0f94-460e-8398-03c98ab6616c.png)
Searches for employees of a company on linkedin using [PuppeteerSharp](https://github.com/hardkoded/puppeteer-sharp) and outputs a list composed of full names and possible emails.<br />
It's possible to create builds for Windows, Linux and macOS since the project is done in .NET Core 6.

## How to use
| Argument | Description |
| ------------- | ------------- |
| -e | Your Linkedin email |
| -p | Your Linkedin password |
| -c | This is the company name, you can find this value by visiting the company's page and checking the URL. Example: https://www.linkedin.com/company/twitter/ |
| -d | The company's domain |

```
LinkedinEmails.exe -e=<email> -p=<password> -c=twitter -d=twitter.com
```
