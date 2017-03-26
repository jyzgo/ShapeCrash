//  Created by Aubrey Falconer on 12/15/12
//  Copyright (c) 2012 PlayXpert. All rights reserved.

#include "KochavaUnitySupport.h"

char* AutonomousStringCopy (const char* string)
{
	if (string == NULL)
	return NULL;
	
	char* res = (char*)malloc(strlen(string) + 1);
	strcpy(res, string);
	return res;
}

extern "C" {
	char* GetExternalKochavaDeviceIdentifiers_iOS(bool suppressIDFV)
	{
		NSString *kochavaInfo = [[KochavaUnitySupport sharedManager] returnKochavaDeviceIdentifiers:suppressIDFV];
		
		const char* jsonChar = AutonomousStringCopy([kochavaInfo UTF8String]);
		return AutonomousStringCopy(jsonChar);
	}
}

extern "C" {
	void GetExternalKochavaInfo_iOS(bool loggingEnabled, bool suppressIDFV, bool gatheriAdInfo, int iAdAttributionAttempts, int iAdAttributionWait, int iAdRetryWait, char *cookieCollectionTargets)
	{
		[[KochavaUnitySupport sharedManager] returnKochavaInfo:loggingEnabled:suppressIDFV:gatheriAdInfo:iAdAttributionAttempts:iAdAttributionWait:iAdRetryWait];
	}
}

extern "C" {
	bool GetExternalKochavaLocationApproved_iOS()
	{
		return [[KochavaUnitySupport sharedManager] isLocationServicesApproved];
	}
	
extern "C" {
	void GetExternalLocationReport_iOS(bool loggingEnabled, int accuracy, int timeout)
	{
		[[KochavaUnitySupport sharedManager] obtainDeviceLocation:loggingEnabled:accuracy:timeout];
	}
}

}

