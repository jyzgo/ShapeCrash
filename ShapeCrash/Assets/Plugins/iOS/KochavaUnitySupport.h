//
//  KochavaUnitySupport.h
//  KochavaUnitySupport
//
//  Created by David Bardwick on 12/3/12.
//  Copyright (c) 2012 PlayXpert. All rights reserved.
//

#import <Foundation/Foundation.h>
#import <CoreLocation/CoreLocation.h>

#ifdef IBEACON
@protocol KochavaUnityiBeaconManagerDelegate;
@protocol KochavaUnityiBeaconManagerDelegate <NSObject>
@optional
- (void) locationUpdate:(CLLocation*)newLocation;
- (void) iBeaconBarrierCrossed:(NSDictionary*)iBeaconBarrierAction;
@end
#endif

@protocol KochavaUnityLocationManagerDelegate;
@protocol KochavaUnityLocationManagerDelegate <NSObject>
@optional
- (void) currentLocationUpdate:(NSDictionary*)newLocation;
@end

@protocol KochavaiAdAttributionDelegate;
@protocol KochavaiAdAttributionDelegate <NSObject>
@optional
- (void) iAdAttributionData:(NSDictionary*)iAdAttributionPayload :(bool)isUnknown;
@end

#ifdef IBEACON
@interface KochavaUnitySupport : NSObject <KochavaUnityiBeaconManagerDelegate, KochavaUnityLocationManagerDelegate, KochavaiAdAttributionDelegate>
#else
@interface KochavaUnitySupport : NSObject <KochavaUnityLocationManagerDelegate, KochavaiAdAttributionDelegate>
#endif

+(KochavaUnitySupport *) sharedManager;

- (NSString *) returnKochavaDeviceIdentifiers:(bool)suppressIDFV;
- (void) returnKochavaInfo:(bool)_loggingEnabled :(bool)suppressIDFV :(bool)gatheriAdInfo :(int)attributionAttempts :(int)attributionWait :(int)retryWait;
- (bool) isLocationServicesApproved;
- (void) obtainDeviceLocation :(bool)_loggingEnabled :(int)accuracy :(int)timeout;

#ifdef IBEACON
- (void) startiBeaconMonitoring:(bool)loggingEnabled;
- (void) monitoriBeacons:(const char *)beacons;
#endif

@end


