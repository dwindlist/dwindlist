# dwindlist

A to-do list webapp that encourages breaking down tasks.

Note: I'm still learning Git, so I might've accidentally duped some commits. Oops.

## Write-up

You can view the design decisions and implementation write-up [here](https://gist.github.com/ChuseCubr/10883566d0c3fffdfd101c9662f4e331).

## Usage

This application uses a database. This uses Code First, so to set it up, run the following:

```pwsh
cd dwindlist
dotnet ef database update
```

To run, use `dotnet run` or `dotnet watch`
