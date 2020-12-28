#include <stdio.h>

void main(void)
{
	int nData = 10;
	int *pnData = &nData;
	int **ppnData = &pnData;
	int ***pppnData = &ppnData;

	printf("%p\n", **pppnData);
	printf("%p\n", *ppnData);
	printf("%p\n", pnData);

	printf("\n");

	printf("nData의 값 : %i\n", nData);
	printf("nData의 주소 : %p\n", &nData);

	printf("\n");

	printf("pnData : %p\n", pnData);
	printf("*pnData : %p\n", *pnData);
	printf("&pnData : %p\n", &pnData);
	
	printf("\n");

	printf("ppnData : %p\n", ppnData);
	printf("*ppnData : %p\n", *ppnData);
	printf("**ppnData : %p\n", **ppnData);
	printf("&ppnData : %p\n", &ppnData);
	
	printf("\n");

	printf("pppnData : %p\n", pppnData);
	printf("*pppnData : %p\n", *pppnData);
	printf("**pppnData : %p\n", **pppnData);
	printf("***pppnData : %p\n", ***pppnData);
	printf("&pppnData : %p\n", &pppnData);


	/*
	00B4F7D0
	00B4F7D0
	00B4F7D0

	nData의 값 : 10
	nData의 주소 : 00B4F7D0

	pnData : 00B4F7D0
	*pnData : 0000000A
	&pnData : 00B4F7C4

	ppnData : 00B4F7C4
	*ppnData : 00B4F7D0
	**ppnData : 0000000A
	&ppnData : 00B4F7B8

	pppnData : 00B4F7B8
	*pppnData : 00B4F7C4
	**pppnData : 00B4F7D0
	***pppnData : 0000000A
	&pppnData : 00B4F7AC
	*/

}