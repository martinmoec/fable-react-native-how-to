module.exports = {
    entry: "src/App.fsproj",
    outDir: "out",
    babel: {
      presets: [["@babel/preset-env", { modules: "commonjs" }]],
      sourceMaps: false,
    },
    // The `onCompiled` hook (optional) is raised after each compilation
    onCompiled() {
        console.log("Compilation finished!")
    }
  }