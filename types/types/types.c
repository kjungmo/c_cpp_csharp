#include <stdio.h>

int main()
{
    int num = 0;
    int size;

    size = sizeof num;
    printf("size of num is : %d\n", size);

    printf("char: %\d, short: %d, int: %d, long: %d, long long: %d, "
    "float: %d, double: %d\n,12.3f: %d, 12.3: %d\n",
        sizeof(char),
        sizeof(short),
        sizeof(int),
        sizeof(long),
        sizeof(long long),
        sizeof(float),
        sizeof(double),
        sizeof(12.3f),
        sizeof(12.3)
    );

    return 0;
}