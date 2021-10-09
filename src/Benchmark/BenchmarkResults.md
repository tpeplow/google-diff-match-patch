#Baseline Results

```
|                               Method |      Mean |    Error |   StdDev |    Gen 0 |    Gen 1 | Allocated |
|------------------------------------- |----------:|---------:|---------:|---------:|---------:|----------:|
|   CheckLines_True_SpeedOptimize_True |  92.72 ms | 1.851 ms | 2.655 ms | 833.3333 | 333.3333 |      6 MB |
|  CheckLines_True_SpeedOptimize_False |  91.17 ms | 1.706 ms | 2.706 ms | 833.3333 | 166.6667 |      6 MB |
|  CheckLines_False_SpeedOptimize_True | 232.68 ms | 4.490 ms | 6.440 ms | 666.6667 |        - |      5 MB |
| CheckLines_False_SpeedOptimize_False | 232.65 ms | 4.564 ms | 6.545 ms | 666.6667 |        - |      6 MB |
```
# Current benchmark

```
|                              Method |      Mean |    Error |   StdDev |    Gen 0 |    Gen 1 | Allocated |
|------------------------------------ |----------:|---------:|---------:|---------:|---------:|----------:|
|  CheckLines_True_SpeedOptimize_True |  87.77 ms | 0.743 ms | 0.620 ms | 666.6667 | 166.6667 |      4 MB |
| CheckLines_False_SpeedOptimize_True | 223.07 ms | 4.229 ms | 4.153 ms | 333.3333 |        - |      4 MB |
```