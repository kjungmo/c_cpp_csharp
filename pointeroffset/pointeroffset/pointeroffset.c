#include <stdio.h>
#include <malloc.h>
#include <string.h>

void main(void)
{
	int *pnData = NULL;
	pnData = (int*)malloc(sizeof(int) * 4);
	memset(pnData, 0, sizeof(int) * 4);
	printf("%p\n", pnData);
	printf("%p\n", pnData + 1);
	printf("%p\n", pnData + 2);
	printf("%p\n", pnData + 3);
	free(pnData);

	//int **pnData = NULL;
	//*pnData = (int**)malloc(sizeof(int*) * 4);
	//memset(*pnData, 0, sizeof(int*) * 4);

	//printf("%p\n", *pnData);
	//printf("%p\n", *pnData + 1);
	//printf("%p\n", *pnData + 2);
	//printf("%p\n", *pnData + 3);
	//free(pnData);
}
