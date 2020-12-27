#include <stdio.h>

void main(void)
{
	int nData = 10;
	int *pnData = &nData;

	printf("%d, %d\n", nData, *pnData);
	printf("%p, %p\n", &nData, pnData);

	*pnData = 20;
	printf("%d\n", nData);
}
