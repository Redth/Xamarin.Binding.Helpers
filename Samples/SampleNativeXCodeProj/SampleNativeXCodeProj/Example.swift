//
//  Example.swift
//  SampleNativeXCodeProj
//
//  Created by Jonathan Dick on 2021-01-27.
//

import Foundation

@objc
public class Example : NSObject
{
    @objc
    public static func Ping() -> NSString
    {
        return "Pong"
    }
}
