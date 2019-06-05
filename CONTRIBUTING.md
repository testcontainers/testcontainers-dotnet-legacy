# Contribution guide

## Before sending a PR

### Code formatting

Please adhere to `.editorconfig` for all new code. It contains rules for naming conventions and formatting style 
(spacing, indentation, modifier order, etc).

To automatically format the code according to `.editorconfig`, install and run the dotnet formatting tool.

```
# installs the tool globally on your computer
dotnet tool install -g dotnet-format

# formats the code
dotnet format
```
