// Copyright (c) Microsoft. All rights reserved.

use std::str::FromStr;
use std::fmt;

pub const API_VERSION: Version = Version::Version2018_12_30;

#[derive(PartialOrd, PartialEq)]
pub enum Version {
    Version2018_06_28,
    Version2018_12_30
}

impl FromStr for Version {
    type Err = ();

    fn from_str(s: &str) -> Result<Version, ()> {
        match s {
            "2018-06-28" => Ok(Version::Version2018_06_28),
            "2018-12-30" => Ok(Version::Version2018_12_30),
            _ => Err(()),
        }
    }
}

impl fmt::Display for Version {
    fn fmt(&self, f: &mut fmt::Formatter) -> fmt::Result {
        match *self 
        {
            Version::Version2018_06_28 => write!(f, "2018-06-28"),
            Version::Version2018_12_30 => write!(f, "2018-12-30"),
        }
    }
}