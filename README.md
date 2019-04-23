# Project for the final bachelor thesis

## Steps

### Local

1. Install python 3.6.8
1. git clone <https://github.com/pythonnet/pythonnet>
1. python3 setup.py bdist_wheel --xplat
1. copy Python.Runtime.dll from build folder pythonnet/build/lib.{your platform}-3.6/netcoreapp2.0/Python.Runtime.dll
1. pip install numpy rake_nltk sklearn wheel pythonnet
1. Build the rest with Visual Studio
1. Install MSSQL and Redis (easiest with docker)
1. Configure
1. Run everything