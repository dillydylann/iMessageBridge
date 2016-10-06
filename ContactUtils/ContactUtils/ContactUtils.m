//
//  ContactUtils.m
//  ContactUtils
//
//  Created by Dylan Briedis on 10/1/16.
//  Copyright © 2016 Dylan Briedis. All rights reserved.
//

#import "ContactUtils.h"

ABPerson* getPerson(ABAddressBook *addressBook, NSString *number) {
    if (number.length == 10)
    {
        ABSearchElement *search1 = [ABPerson searchElementForProperty:kABPhoneProperty label:nil key:nil value:[number substringWithRange:NSMakeRange(0, 3)] comparison:kABContainsSubStringCaseInsensitive];
        ABSearchElement *search2 = [ABPerson searchElementForProperty:kABPhoneProperty label:nil key:nil value:[number substringWithRange:NSMakeRange(3, 3)] comparison:kABContainsSubStringCaseInsensitive];
        ABSearchElement *search3 = [ABPerson searchElementForProperty:kABPhoneProperty label:nil key:nil value:[number substringWithRange:NSMakeRange(6, 4)] comparison:kABContainsSubStringCaseInsensitive];
        ABSearchElement *search = [ABSearchElement searchElementForConjunction:kABSearchAnd children:@[search1, search2, search3]];
        NSArray *contacts = [addressBook recordsMatchingSearchElement:search];
        if (contacts.count > 0)
            return [contacts firstObject];
        else
            return nil;
    }
    else
    {
        ABSearchElement *search = [ABPerson searchElementForProperty:kABPhoneProperty label:nil key:nil value:number comparison:kABContainsSubStringCaseInsensitive];
        NSArray *contacts = [addressBook recordsMatchingSearchElement:search];
        if (contacts.count > 0)
            return [contacts firstObject];
        else
            return nil;
    }
}

Person* GetPersonFromNumber(char* number) {
    NSString *phoneNumber = [NSString stringWithUTF8String:number];
    NSString *name = nil;
    ABAddressBook *addressBook = [ABAddressBook sharedAddressBook];
    ABPerson *person = getPerson(addressBook, phoneNumber);
    if (person != nil)
    {
        NSString *firstName = [person valueForProperty:kABFirstNameProperty];
        NSString *lastName = [person valueForProperty:kABLastNameProperty];
        name = @"";
        if (firstName != nil)
            name = [name stringByAppendingString:firstName];
        if (firstName != nil && lastName != nil)
            name = [name stringByAppendingString:@" "];
        if (lastName != nil)
            name = [name stringByAppendingString:lastName];
        Person* result = (Person*)malloc(sizeof(Person));
        result->name = name;
        result->picture = [person imageData];
        result->pictureLength = [result->picture length];
        return result;
    }
    return nil;
}

Person* GetPersonFromEmail(char* email) {
    NSString *name = nil;
    ABAddressBook *addressBook = [ABAddressBook sharedAddressBook];
    ABSearchElement *search = [ABPerson searchElementForProperty:kABEmailProperty label:nil key:nil value:[NSString stringWithCString:email encoding:NSUTF8StringEncoding] comparison:kABContainsSubStringCaseInsensitive];
    NSArray *contacts = [addressBook recordsMatchingSearchElement:search];
    if (contacts.count > 0)
    {
        ABPerson *person = [contacts firstObject];
        NSString *firstName = [person valueForProperty:kABFirstNameProperty];
        NSString *lastName = [person valueForProperty:kABLastNameProperty];
        name = @"";
        if (firstName != nil)
            name = [name stringByAppendingString:firstName];
        if (firstName != nil && lastName != nil)
            name = [name stringByAppendingString:@" "];
        if (lastName != nil)
            name = [name stringByAppendingString:lastName];
        Person* result = (Person*)malloc(sizeof(Person));
        result->name = name;
        result->picture = [person imageData];
        result->pictureLength = [result->picture length];
        return result;
    }
    return nil;
}

NSString* GetNameFromPerson(Person* person) {
    return person->name;
}

const void* GetPictureFromPerson(Person* person) {
    return [person->picture bytes];
}

uint GetPictureLengthFromPerson(Person* person) {
    return [person->picture length];
}
