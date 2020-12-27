#include <stdio.h>
#include <string.h>
#include <stdlib.h>

void main(void)
{
	int *pnData = NULL;
	pnData = (int*)malloc(sizeof(int) * 4);
	memset(pnData, 0, sizeof(int) * 4);

	*(pnData) = 10;
	*(pnData + 1) = 20;
	printf("%d\n", *(pnData));
	printf("%d\n", *(pnData + 1));

	free(pnData);

}