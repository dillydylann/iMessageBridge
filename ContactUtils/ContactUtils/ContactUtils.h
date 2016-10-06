//
//  ContactUtils.h
//  ContactUtils
//
//  Created by Dylan Briedis on 10/1/16.
//  Copyright © 2016 Dylan Briedis. All rights reserved.
//

#import <AddressBook/AddressBook.h>
#import <Foundation/Foundation.h>

#define dllexport __attribute__((visibility("default")))

typedef struct {
    NSString* name;
    NSData* picture;
    uint pictureLength;
} Person;


dllexport Person* GetPersonFromNumber(char* number);
dllexport Person* GetPersonFromEmail(char* email);
dllexport NSString* GetNameFromPerson(Person* person);
dllexport const void* GetPictureFromPerson(Person* person);
dllexport uint GetPictureLengthFromPerson(Person* person);
