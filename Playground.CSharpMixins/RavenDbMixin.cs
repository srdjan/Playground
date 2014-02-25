﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Raven.Client;
using Raven.Client.Document;

namespace Playground.CSharpMixins {
  public interface IRavenDbMixin { }

  public static class RavenDbMixin {
    static readonly ConditionalWeakTable<IRavenDbMixin, IDocumentSession> _table;

    static RavenDbMixin() {
      _table = new ConditionalWeakTable<IRavenDbMixin, IDocumentSession>();
    }
    
    const string RavenDbConnectionString = "RavenDB";
    static IDocumentStore _docStore;
    static IDocumentStore DocumentStore {
      get { return _docStore ?? (_docStore = new DocumentStore { ConnectionStringName = RavenDbConnectionString }.Initialize()); }
    }

    public static List<T> Query<T>(this IRavenDbMixin target, Func<T, bool> predicate) {
      var session = tryGet(target);
      return session.Query<T>().Where(predicate).ToList();
    }

    public static T QuerySingleOrDefault<T>(this IRavenDbMixin target, Func<T, bool> predicate) {
      var session = tryGet(target);
      return session.Query<T>().Where(predicate).ToList().SingleOrDefault();
    }

    public static void Store<T>(this IRavenDbMixin target, T @Object) {
      var session = tryGet(target);
      session.Store(@Object);
    }

    public static T Load<T>(this IRavenDbMixin target, int id) {
      var session = tryGet(target);
      return session.Load<T>(id);
    }

    public static void Save(this IRavenDbMixin target) {
      var session = tryGet(target);
      session.SaveChanges();
    }

    public static IDocumentSession Session(this IRavenDbMixin target) {
      var session = tryGet(target);
      return session;
    }

    static IDocumentSession tryGet(IRavenDbMixin target) {
      IDocumentSession documentSession;
      var exist = _table.TryGetValue(target, out documentSession);
      if (exist) {
        return documentSession;
      }
      _table.Add(target, DocumentStore.OpenSession());
      _table.TryGetValue(target, out documentSession);
      return documentSession;
    }
  }
}